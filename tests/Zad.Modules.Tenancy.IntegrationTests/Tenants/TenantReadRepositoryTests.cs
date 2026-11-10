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

    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Middleware_Returns_404_For_Unknown_Host_And_403_For_Suspended_Tenant()
    {
        var ct = TestContext.Current.CancellationToken;
        using var unknownClient = Factory.CreateClient();
        unknownClient.DefaultRequestHeaders.Host = "unknown.localhost";

        var unknownResponse = await unknownClient.GetAsync("/api/catalog/products", ct);
        unknownResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var platformClient = Factory.CreatePlatformClient();
        var suspendResponse = await platformClient.PostAsync($"/api/platform/tenants/{Tenants.B.Id}/suspend", null, ct);
        suspendResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var bravoClient = Factory.CreateClientForTenant(Tenants.B);
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

