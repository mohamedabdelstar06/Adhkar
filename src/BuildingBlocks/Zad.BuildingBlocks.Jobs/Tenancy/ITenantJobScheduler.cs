using System.Linq.Expressions;

namespace Zad.BuildingBlocks.Jobs.Tenancy;

public interface ITenantJobScheduler
{
    public string Enqueue<TJob>(Expression<Func<TJob, Task>> call);

    public string Schedule<TJob>(Expression<Func<TJob, Task>> call, TimeSpan delay);

    public string EnqueueForTenant<TJob>(Guid tenantId, Expression<Func<TJob, Task>> call);
}
