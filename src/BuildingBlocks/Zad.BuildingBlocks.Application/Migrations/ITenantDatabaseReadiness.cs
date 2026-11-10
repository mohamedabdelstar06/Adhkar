namespace Zad.BuildingBlocks.Application.Migrations;

public interface ITenantDatabaseReadiness
{
    public Task<bool> IsReadyAsync(Guid databaseId, CancellationToken cancellationToken = default);
}
