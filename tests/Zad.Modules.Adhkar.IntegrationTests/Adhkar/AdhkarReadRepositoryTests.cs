using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Adhkar.Application;
using Zad.Modules.Adhkar.Application.Adhkar;
using Zad.Modules.Adhkar.Application.Rotation;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using Zad.Modules.Adhkar.Domain.Adhkar;
using Zad.Modules.Adhkar.Infrastructure.Persistence;
using Zad.TestSupport.Categories;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;
using Zad.TestSupport.Host;
using AdhkarEntity = Zad.Modules.Adhkar.Domain.Adhkar.Adhkar;

namespace Zad.Modules.Adhkar.IntegrationTests.Adhkar;

[Collection(IntegrationTestCollection.Name)]
public sealed class AdhkarReadRepositoryTests(SqlServerFixture sqlServer) : IntegrationTestBase(sqlServer)
{
    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task ReadRepository_Isolates_Adhkar_And_Rotation_State_Per_Tenant()
    {
        var ct = TestContext.Current.CancellationToken;

        var alphaDhikr = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var service = sp.GetRequiredService<AdhkarManagementService>();
            var created = await service.CreateAsync(
                new CreateAdhkarRequest("سبحان الله", "Glory be to Allah", "Muslim 2694", AdhkarCategories.Tasbih, 1, true),
                ct);
            created.IsSuccess.Should().BeTrue();
            return created.Value;
        });

        var bravoDhikr = await WithTenantScopeAsync(Tenants.B, async sp =>
        {
            var service = sp.GetRequiredService<AdhkarManagementService>();
            var created = await service.CreateAsync(
                new CreateAdhkarRequest("الحمد لله", "Praise be to Allah", "Muslim 2694", AdhkarCategories.Tahmid, 1, true),
                ct);
            created.IsSuccess.Should().BeTrue();
            return created.Value;
        });

        await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var rotation = sp.GetRequiredService<AdhkarRotationService>();
            var rotated = await rotation.RotateAsync(ct);
            rotated.IsSuccess.Should().BeTrue();
            rotated.Value.Id.Should().Be(alphaDhikr.Id);

            var repo = sp.GetRequiredService<IAdhkarReadRepository>();
            (await repo.ListAsync(includeInactive: true, ct)).Should().ContainSingle(a => a.Id == alphaDhikr.Id);
            (await repo.GetByIdAsync(bravoDhikr.Id, ct)).Should().BeNull();
            (await repo.GetRotationCurrentIdAsync(ct)).Should().Be(alphaDhikr.Id);
        });

        await WithTenantScopeAsync(Tenants.B, async sp =>
        {
            var repo = sp.GetRequiredService<IAdhkarReadRepository>();
            (await repo.ListAsync(includeInactive: true, ct)).Should().ContainSingle(a => a.Id == bravoDhikr.Id);
            (await repo.GetFirstActiveAsync(ct))!.Id.Should().Be(bravoDhikr.Id);
            (await repo.GetRotationCurrentIdAsync(ct)).Should().BeNull();
        });
    }

    [Fact]
    [Trait("Category", TestCategories.Rollback)]
    public async Task ExecuteInTransactionAsync_Rolls_Back_Uncommitted_Adhkar()
    {
        var ct = TestContext.Current.CancellationToken;

        var result = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var repo = sp.GetRequiredService<IAdhkarRepository>();
            var uow = sp.GetRequiredService<IAdhkarUnitOfWork>();

            return await uow.ExecuteInTransactionAsync(
                async token =>
                {
                    var dhikr = AdhkarEntity.Create("الله أكبر", "Allah is Greatest", "Muslim", AdhkarCategories.Takbir, 9, true).Value;
                    repo.Add(dhikr);
                    (await uow.SaveChangesAsync(token)).IsSuccess.Should().BeTrue();
                    return Result.Failure(Error.Failure("test.rollback", "Force rollback"));
                },
                ct);
        });

        result.IsFailure.Should().BeTrue();

        await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var db = sp.GetRequiredService<AdhkarDbContext>();
            (await db.Adhkar.CountAsync(ct)).Should().Be(0);
        });
    }

    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Http_Endpoints_Support_Crud_And_Cross_Tenant_Isolation()
    {
        var ct = TestContext.Current.CancellationToken;
        using var alphaClient = Factory.CreateClientForTenant(Tenants.A);
        using var bravoClient = Factory.CreateClientForTenant(Tenants.B);

        var createResponse = await alphaClient.PostAsJsonAsync(
            "/api/adhkar",
            new CreateAdhkarRequest("سبحان الله", "Glory be to Allah", "Muslim", AdhkarCategories.Tasbih, 1, true),
            ct);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<AdhkarDto>(ct);

        var alphaCurrent = await alphaClient.GetAsync("/api/adhkar/current", ct);
        alphaCurrent.StatusCode.Should().Be(HttpStatusCode.OK);

        var bravoList = await bravoClient.GetFromJsonAsync<IReadOnlyList<AdhkarDto>>("/api/adhkar", ct);
        bravoList.Should().BeEmpty();

        var bravoGet = await bravoClient.GetAsync($"/api/adhkar/{created!.Id}", ct);
        bravoGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var updateResponse = await alphaClient.PutAsJsonAsync(
            $"/api/adhkar/{created.Id}",
            new UpdateAdhkarRequest("سبحان الله وبحمده", "Glory and praise", "Muslim 2691", AdhkarCategories.Tasbih, 1, true),
            ct);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await alphaClient.DeleteAsync($"/api/adhkar/{created.Id}", ct);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDelete = await alphaClient.GetAsync($"/api/adhkar/{created.Id}", ct);
        afterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

