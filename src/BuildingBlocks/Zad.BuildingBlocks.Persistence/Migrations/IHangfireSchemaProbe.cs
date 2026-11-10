namespace Zad.BuildingBlocks.Persistence.Migrations;

public interface IHangfireSchemaProbe
{
    public Task<bool> IsInstalledAsync(string jobsConnectionString, CancellationToken cancellationToken = default);
}
