using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Contracts;
using Zad.TestSupport.Categories;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;
using Zad.TestSupport.Host;

namespace Zad.Modules.Tenancy.IntegrationTests.Tenants;

[Collection(IntegrationTestCollection.Name)]
public sealed class TenantReadRepositoryTests(SqlServerFixture sqlServer) : IntegrationTestBase(sqlServer)
{
    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task ReadRepository_Resolves_And_Lists_Provisioned_Tenants()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var scope = Factory.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITenantReadRepository>();

        var alpha = await repo.ResolveByHostAsync(Tenants.A.Host, ct);
        alpha.Should().NotBeNull();
        alpha!.Slug.Should().Be("alpha");
        alpha.IsActive.Should().BeTrue();

        var bravoBySlug = await repo.ResolveBySlugAsync("bravo", ct);
        bravoBySlug.Should().NotBeNull();
        bravoBySlug!.TenantId.Should().Be(Tenants.B.Id);

        var active = await repo.ListActiveAsync(ct);
        active.Should().Contain(t => t.TenantId == Tenants.A.Id);
        active.Should().Contain(t => t.TenantId == Tenants.B.Id);

        var all = await repo.ListAllAsync(ct);
        all.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("unknown.localhost")]
    [InlineData("www.localhost")]
    [InlineData("localhost")]
    [InlineData("a.b.localhost")]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Unknown_Reserved_Bare_And_Nested_Hosts_Return_404_For_Both_Page_And_Api(string invalidHost)
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Host = invalidHost;

        var pageResponse = await client.GetAsync("/", ct);
        pageResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        pageResponse.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        var pageBody = await pageResponse.Content.ReadAsStringAsync(ct);
        pageBody.Should().Contain("المستأجر غير موجود");
        pageBody.Should().NotContain("<app-root>");

        var apiResponse = await client.GetAsync("/api/catalog/products", ct);
        apiResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiBody = await apiResponse.Content.ReadAsStringAsync(ct);
        apiBody.Should().Contain(TenancyErrorCodes.TenantNotFound);

        var platformApiOnInvalidHost = await client.GetAsync("/api/platform/tenants", ct);
        platformApiOnInvalidHost.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Platform_Api_Is_Blocked_On_Tenant_Hosts_With_404()
    {
        var ct = TestContext.Current.CancellationToken;

        using var tenantClient = Factory.CreateClientForTenant(Tenants.A);
        var response = await tenantClient.GetAsync("/api/platform/tenants", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(ct);
        body.Should().Contain("platform.not-available");
    }

    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Middleware_Blocks_Suspended_Tenant_Pages_With_403_And_Allows_Platform_Host()
    {
        var ct = TestContext.Current.CancellationToken;

        using var adminClient = Factory.CreateClient();
        adminClient.DefaultRequestHeaders.Host = "admin.localhost";
        var adminPage = await adminClient.GetAsync("/", ct);
        var adminBody = await adminPage.Content.ReadAsStringAsync(ct);
        adminBody.Should().NotContain("المستأجر غير موجود");

        using var platformClient = Factory.CreatePlatformClient();
        var suspendResponse = await platformClient.PostAsync($"/api/platform/tenants/{Tenants.B.Id}/suspend", null, ct);
        suspendResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var bravoClient = Factory.CreateClientForTenant(Tenants.B);
        var suspendedPage = await bravoClient.GetAsync("/", ct);
        suspendedPage.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        suspendedPage.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        var suspendedHtml = await suspendedPage.Content.ReadAsStringAsync(ct);
        suspendedHtml.Should().Contain("تم إيقاف الخدمة");

        var suspendedGet = await bravoClient.GetAsync("/api/catalog/products", ct);
        suspendedGet.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var activateResponse = await platformClient.PostAsync($"/api/platform/tenants/{Tenants.B.Id}/activate", null, ct);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reactivatedGet = await bravoClient.GetAsync("/api/catalog/products", ct);
        reactivatedGet.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Platform_Endpoints_Register_Update_List_And_Delete_Tenant()
    {
        var ct = TestContext.Current.CancellationToken;
        using var platformClient = Factory.CreatePlatformClient();

        var createResponse = await platformClient.PostAsJsonAsync(
            "/api/platform/tenants",
            new ProvisionTenantRequest("charlie", "charlie.localhost", "ar", "UTC"),
            ct);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedTenantResponse>(ct);
        created.Should().NotBeNull();

        using var charlieClient = Factory.CreateClient();
        charlieClient.DefaultRequestHeaders.Host = "charlie.localhost";

        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        var current = await charlieClient.GetFromJsonAsync<TenantInfo>("/api/tenancy/current", jsonOptions, ct);
        current!.TenantId.Should().Be(created!.TenantId);
        current.Slug.Should().Be("charlie");

        var updateResponse = await platformClient.PutAsJsonAsync(
            $"/api/platform/tenants/{created.TenantId}",
            new UpdatePlatformTenantRequest("charlie-new.localhost", "en", "Africa/Cairo"),
            ct);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await platformClient.GetFromJsonAsync<IReadOnlyList<TenantRegistrationDto>>("/api/platform/tenants", ct);
        list.Should().ContainSingle(t => t.TenantId == created.TenantId && t.Host == "charlie-new.localhost");

        var deleteResponse = await platformClient.DeleteAsync($"/api/platform/tenants/{created.TenantId}", ct);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedCheck = await charlieClient.GetAsync("/api/tenancy/current", ct);
        deletedCheck.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record CreatedTenantResponse(Guid TenantId);
}

