using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.MigrationRunner.Cli;
using Zad.Modules.Tenancy.Contracts;
using Zad.Modules.Tenancy.Infrastructure.Databases;

namespace Zad.MigrationRunner.Targets;

public sealed class MigrationTargetSource(IConfiguration configuration)
{
    public async Task<IReadOnlyList<MigrationTarget>> GetAsync(RunnerOptions options, CancellationToken cancellationToken)
    {
        if (options.TenantSlug is not null)
        {
            return [await TenantTargetAsync(options.TenantSlug, cancellationToken)];
        }

        var targets = new List<MigrationTarget>();

        if (options.Includes(DatabaseSelector.Catalog))
        {
            targets.Add(CatalogTarget());
        }

        if (options.Includes(DatabaseSelector.Shared))
        {
            targets.AddRange(SharedTargets());
        }

        if (options.Includes(DatabaseSelector.Jobs))
        {
            targets.Add(JobsTarget());
        }

        return targets;
    }

    private async Task<MigrationTarget> TenantTargetAsync(string slug, CancellationToken cancellationToken)
    {
        await using var provider = Seeding.ModuleServiceProviderFactory.Build(configuration);
        await using var scope = provider.CreateAsyncScope();

        var registry = scope.ServiceProvider.GetRequiredService<TenancyRegistry>();
        await registry.RefreshAsync(cancellationToken);

        var directory = scope.ServiceProvider.GetRequiredService<ITenantDirectory>();
        var tenant = await directory.ResolveBySlugAsync(slug, cancellationToken)
            ?? throw new ArgumentException($"Tenant '{slug}' is not registered in the catalog");

        var descriptor = registry.Find(tenant.DatabaseId)
            ?? throw new ArgumentException($"Database '{tenant.DatabaseId}' for tenant '{slug}' is not registered");

        var connectionString = await registry.ResolveMigratorConnectionStringAsync(tenant.DatabaseId, cancellationToken);

        var kind = descriptor.Kind == TenantDatabaseKind.Dedicated
            ? MigrationTargetKind.Dedicated
            : MigrationTargetKind.Shared;

        return new MigrationTarget(tenant.DatabaseId, kind, descriptor.Name, connectionString);
    }

    private MigrationTarget CatalogTarget()
    {
        var connectionString = Environment.GetEnvironmentVariable("ZAD_MIGRATIONS_CATALOG_CONNECTION_STRING")
            ?? configuration.GetConnectionString("Catalog")
            ?? throw new InvalidOperationException("ConnectionStrings:Catalog is required");

        return new MigrationTarget(PlatformDatabaseIds.Catalog, MigrationTargetKind.Catalog, DatabaseNameOf(connectionString), connectionString);
    }

    private IReadOnlyList<MigrationTarget> SharedTargets()
    {
        var connectionString = Environment.GetEnvironmentVariable("ZAD_MIGRATIONS_SHARED_CONNECTION_STRING")
            ?? configuration.GetConnectionString("Shared");

        if (connectionString is null)
        {
            return [];
        }

        return [new MigrationTarget(PlatformDatabaseIds.Shared, MigrationTargetKind.Shared, DatabaseNameOf(connectionString), connectionString)];
    }

    private MigrationTarget JobsTarget()
    {
        var connectionString = Environment.GetEnvironmentVariable("ZAD_MIGRATIONS_JOBS_CONNECTION_STRING")
            ?? configuration.GetConnectionString("Jobs")
            ?? throw new InvalidOperationException("ConnectionStrings:Jobs is required");

        return new MigrationTarget(PlatformDatabaseIds.Jobs, MigrationTargetKind.Jobs, DatabaseNameOf(connectionString), connectionString);
    }

    private static string DatabaseNameOf(string connectionString) =>
        new SqlConnectionStringBuilder(connectionString).InitialCatalog;
}
