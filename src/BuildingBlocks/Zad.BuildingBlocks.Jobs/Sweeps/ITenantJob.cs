namespace Zad.BuildingBlocks.Jobs.Sweeps;

public interface ITenantJob
{
    public Task RunAsync(CancellationToken cancellationToken);
}
