using Zad.BuildingBlocks.Application.Tenancy;

namespace Zad.BuildingBlocks.Persistence.Tenancy;

public sealed class UnresolvedTenantContext : ITenantContext
{
    public static readonly UnresolvedTenantContext Instance = new();

    private UnresolvedTenantContext()
    {
    }

    public bool IsResolved => false;

    public Guid TenantId => throw new InvalidOperationException(
        "Tenant context has not been resolved. Multi-tenant operations require a valid tenant context");
}
