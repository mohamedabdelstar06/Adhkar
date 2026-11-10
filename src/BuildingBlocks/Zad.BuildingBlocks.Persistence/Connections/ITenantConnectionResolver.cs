namespace Zad.BuildingBlocks.Persistence.Connections;

public interface ITenantConnectionResolver
{
    public string ResolveApplicationConnectionString(Guid tenantId);

    public string ResolveApplicationConnectionStringForDatabase(Guid databaseId);

    public Task<string> ResolveMigratorConnectionStringAsync(Guid databaseId, CancellationToken cancellationToken = default);
}
