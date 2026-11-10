using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Application.Security;
using Zad.Modules.Tenancy.Application.Tenants;

namespace Zad.Modules.Tenancy.Infrastructure.Databases;

public sealed class TenancyRegistry(
    IServiceScopeFactory scopeFactory,
    IConnectionStringProtector protector) : IDatabaseRegistry, ITenantConnectionResolver, ITenancyCacheRefresher
{
    private Snapshot _snapshot = Snapshot.Empty;

    public IReadOnlyList<DatabaseDescriptor> List() => _snapshot.Databases;

    public DatabaseDescriptor? Find(Guid databaseId) =>
        _snapshot.Databases.FirstOrDefault(database => database.Id == databaseId);

    public string ResolveApplicationConnectionString(Guid tenantId)
    {
        if (!_snapshot.TenantToDatabase.TryGetValue(tenantId, out var databaseId))
        {
            throw new InvalidOperationException($"Tenant '{tenantId}' is not registered in the catalog");
        }

        return ResolveApplicationConnectionStringForDatabase(databaseId);
    }

    public string ResolveApplicationConnectionStringForDatabase(Guid databaseId) =>
        _snapshot.ApplicationConnectionStrings.TryGetValue(databaseId, out var connectionString)
            ? connectionString
            : throw new InvalidOperationException($"Database '{databaseId}' has no application connection string");

    public Task<string> ResolveMigratorConnectionStringAsync(Guid databaseId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return _snapshot.MigratorConnectionStrings.TryGetValue(databaseId, out var connectionString)
            ? Task.FromResult(connectionString)
            : Task.FromException<string>(
                new InvalidOperationException($"Database '{databaseId}' has no migrator connection string"));
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var databases = scope.ServiceProvider.GetRequiredService<IDatabaseReadRepository>();
        var tenants = scope.ServiceProvider.GetRequiredService<ITenantReadRepository>();

        var rawDatabases = await databases.ListAsync(cancellationToken);
        var registered = await tenants.ListAllAsync(cancellationToken);

        var descriptors = rawDatabases.Select(d => new DatabaseDescriptor(
            d.Id,
            string.Equals(d.Kind, "Dedicated", StringComparison.OrdinalIgnoreCase) ? TenantDatabaseKind.Dedicated : TenantDatabaseKind.Shared,
            d.Name,
            string.Equals(d.Status, "Online", StringComparison.OrdinalIgnoreCase) ? TenantDatabaseStatus.Online
                : string.Equals(d.Status, "Retired", StringComparison.OrdinalIgnoreCase) ? TenantDatabaseStatus.Retired
                : TenantDatabaseStatus.Provisioning
        )).ToList();

        var applicationConnectionStrings = new Dictionary<Guid, string>();
        var migratorConnectionStrings = new Dictionary<Guid, string>();

        foreach (var descriptor in descriptors)
        {
            var strings = await databases.GetConnectionStringsAsync(descriptor.Id, cancellationToken);
            if (strings is null)
            {
                continue;
            }

            applicationConnectionStrings[descriptor.Id] = protector.Unprotect(strings.App);
            migratorConnectionStrings[descriptor.Id] = protector.Unprotect(strings.Migrator);
        }

        var tenantToDatabase = registered.ToDictionary(tenant => tenant.TenantId, tenant => tenant.DatabaseId);

        _snapshot = new Snapshot(descriptors, tenantToDatabase, applicationConnectionStrings, migratorConnectionStrings);
    }

    private sealed record Snapshot(
        IReadOnlyList<DatabaseDescriptor> Databases,
        IReadOnlyDictionary<Guid, Guid> TenantToDatabase,
        IReadOnlyDictionary<Guid, string> ApplicationConnectionStrings,
        IReadOnlyDictionary<Guid, string> MigratorConnectionStrings)
    {
        public static readonly Snapshot Empty = new(
            [],
            new Dictionary<Guid, Guid>(),
            new Dictionary<Guid, string>(),
            new Dictionary<Guid, string>());
    }
}
