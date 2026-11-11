using Zad.BuildingBlocks.Application.Tenancy;
using Zad.Modules.Tenancy.Contracts;

namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed class CurrentTenantReader(
    ITenantContext tenantContext,
    ITenantReadRepository readRepository)
{
    public Task<TenantInfo?> GetAsync(CancellationToken cancellationToken = default) =>
        tenantContext.IsResolved
            ? readRepository.GetAsync(tenantContext.TenantId, cancellationToken)
            : Task.FromResult<TenantInfo?>(null);
}
