using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.MigrationRunner.Execution;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Application.Security;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Contracts;
using Zad.Modules.Tenancy.Domain.Databases;
using Zad.Modules.Tenancy.Infrastructure.Databases;

namespace Zad.MigrationRunner.Tenancy;

public sealed class TenantOnboarding(IConfiguration configuration, RunnerLog log)
{
    private const string SharedDatabaseName = "ZadShared01";

    public async Task<bool> RegisterAsync(
        string slug,
        string host,
        string defaultLanguage,
        string timeZone,
        CancellationToken cancellationToken)
    {
        var sharedConnectionString = configuration.GetConnectionString("Shared");
        if (sharedConnectionString is null)
        {
            log.Error("ConnectionStrings:Shared is missing; the tenant was not registered");
            return false;
        }

        await using var provider = Seeding.ModuleServiceProviderFactory.Build(configuration);
        await using var scope = provider.CreateAsyncScope();

        var protector = scope.ServiceProvider.GetRequiredService<IConnectionStringProtector>();
        var provisioning = scope.ServiceProvider.GetRequiredService<TenantProvisioningService>();
        var registry = scope.ServiceProvider.GetRequiredService<TenancyRegistry>();

        var databaseResult = await provisioning.RegisterDatabaseAsync(
            new RegisterDatabaseRequest(
                SharedDatabaseName,
                DatabaseKind.Shared,
                protector.Protect(sharedConnectionString),
                protector.Protect(sharedConnectionString)),
            cancellationToken);

        if (databaseResult.IsFailure)
        {
            log.Error($"Registering the shared database failed: {databaseResult.Error.Message}");
            return false;
        }

        var tenantResult = await provisioning.RegisterAsync(
            new RegisterTenantRequest(slug, host, databaseResult.Value, defaultLanguage, timeZone),
            cancellationToken);

        if (tenantResult.IsFailure)
        {
            log.Error($"Registering tenant '{slug}' failed: {tenantResult.Error.Message}");
            return false;
        }

        var activateResult = await provisioning.ActivateAsync(tenantResult.Value, cancellationToken);
        if (activateResult.IsFailure &&
            activateResult.Error.Code != TenancyErrorCodes.TenantAlreadyActive)
        {
            log.Error($"Activating tenant '{slug}' failed: {activateResult.Error.Message}");
            return false;
        }

        await registry.RefreshAsync(cancellationToken);

        log.Info($"Tenant '{slug}' is active on {SharedDatabaseName} and answers on host {host}");
        return true;
    }
}
