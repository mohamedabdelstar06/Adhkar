using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.TestSupport.Databases;
using Zad.TestSupport.Tenancy;

namespace Zad.TestSupport.Host;

public abstract class IntegrationTestBase(SqlServerFixture sqlServer) : IAsyncLifetime
{
    private TestDatabaseSet? _databases;
    private ZadApplicationFactory? _factory;

    protected TestDatabaseSet Databases => _databases ?? throw new InvalidOperationException("Databases are not initialized.");
    protected ZadApplicationFactory Factory => _factory ?? throw new InvalidOperationException("Factory is not initialized.");
    protected ProvisionedTenants Tenants { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        var (databases, factory) = await sqlServer.EnsureSharedHostAsync();
        _databases = databases;
        _factory = factory;
        Tenants = await databases.ResetAndProvisionAsync(factory.Services);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    protected async Task ResetStateAsync(CancellationToken cancellationToken = default)
    {
        Tenants = await Databases.ResetAndProvisionAsync(Factory.Services, cancellationToken);
    }

    protected async Task WithTenantScopeAsync(
        TenantHandle tenant,
        Func<IServiceProvider, Task> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var setter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
        setter.Set(tenant.Id);

        await action(scope.ServiceProvider);
    }

    protected async Task<T> WithTenantScopeAsync<T>(
        TenantHandle tenant,
        Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var setter = scope.ServiceProvider.GetRequiredService<ITenantContextSetter>();
        setter.Set(tenant.Id);

        return await action(scope.ServiceProvider);
    }
}
