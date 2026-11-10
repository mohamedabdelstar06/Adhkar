using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Jobs.Tenancy;
using Zad.Modules.Tenancy.Contracts;

namespace Zad.BuildingBlocks.Jobs.Sweeps;

public sealed class TenantSweep<TJob>(
    ITenantDirectory tenants,
    ITenantJobScheduler scheduler,
    ILogger<TenantSweep<TJob>> logger)
    where TJob : ITenantJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var active = await tenants.ListActiveAsync(cancellationToken);
        var failed = 0;

        foreach (var tenant in active)
        {
            try
            {
                scheduler.EnqueueForTenant<TJob>(tenant.TenantId, job => job.RunAsync(CancellationToken.None));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                failed++;
                logger.LogError(ex, "Sweep {JobName} could not enqueue tenant {TenantId}", typeof(TJob).Name, tenant.TenantId);
            }
        }

        logger.LogInformation(
            "Sweep {JobName} enqueued {Enqueued} and failed {Failed}",
            typeof(TJob).Name,
            active.Count - failed,
            failed);
    }
}
