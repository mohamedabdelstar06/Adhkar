using Zad.BuildingBlocks.Application.Tenancy;

namespace Zad.BuildingBlocks.Persistence.Tenancy;

public sealed class TenantContext : ITenantContext, ITenantContextSetter
{
    private Guid? _tenantId;

    public bool IsResolved => _tenantId.HasValue;

    public Guid TenantId => _tenantId
        ?? throw new InvalidOperationException("Tenant context has not been resolved. Multi-tenant operations require a valid tenant context");

    public void Set(Guid tenantId)
    {
        if (_tenantId.HasValue && _tenantId.Value != tenantId)
        {
            throw new InvalidOperationException("Tenant context cannot be modified once set");
        }

        _tenantId = tenantId;
    }
}
