using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.TestSupport.Categories;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;
using Zad.TestSupport.Host;

namespace Zad.Modules.Tenancy.IntegrationTests.Databases;

[Collection(IntegrationTestCollection.Name)]
public sealed class DatabaseReadRepositoryTests(SqlServerFixture sqlServer) : IntegrationTestBase(sqlServer)
{
    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task ReadRepository_Lists_Registered_Online_Shared_Database()
    {
        var ct = TestContext.Current.CancellationToken;

        await using var scope = Factory.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IDatabaseReadRepository>();

        var databases = await repo.ListAsync(ct);

        var shared = databases.Should().ContainSingle(db => db.Name == "Shared01").Subject;
        shared.Kind.Should().Be("Shared");
        shared.Status.Should().Be("Online");

        var connectionStrings = await repo.GetConnectionStringsAsync(shared.Id, ct);
        connectionStrings.Should().NotBeNull();
        connectionStrings!.App.Should().NotBeNullOrWhiteSpace();
        connectionStrings.Migrator.Should().NotBeNullOrWhiteSpace();
    }
}

