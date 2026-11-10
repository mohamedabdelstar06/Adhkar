using System.Linq.Expressions;
using Hangfire;
using Zad.BuildingBlocks.Application.Localization;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Application.Tenancy;

namespace Zad.BuildingBlocks.Jobs.Tenancy;

public sealed class TenantJobScheduler(
    IBackgroundJobClient backgroundJobs,
    ITenantContext tenantContext,
    ICorrelationContext correlationContext,
    ICultureContext cultureContext) : ITenantJobScheduler
{
    public string Enqueue<TJob>(Expression<Func<TJob, Task>> call) =>
        WithSnapshot(() => backgroundJobs.Enqueue(call));

    public string Schedule<TJob>(Expression<Func<TJob, Task>> call, TimeSpan delay) =>
        WithSnapshot(() => backgroundJobs.Schedule(call, delay));

    public string EnqueueForTenant<TJob>(Guid tenantId, Expression<Func<TJob, Task>> call) =>
        WithSnapshot(() => backgroundJobs.Enqueue(call), tenantId);

    private string WithSnapshot(Func<string> enqueue, Guid? tenantOverride = null)
    {
        var tenantId = tenantOverride ?? (tenantContext.IsResolved ? tenantContext.TenantId : null);

        using var scope = JobContextCapture.Begin(
            new JobContextSnapshot(tenantId, correlationContext.CorrelationId, cultureContext.Language));

        return enqueue();
    }
}
