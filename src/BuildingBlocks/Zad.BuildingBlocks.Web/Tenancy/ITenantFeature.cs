using Zad.Modules.Tenancy.Contracts;

namespace Zad.BuildingBlocks.Web.Tenancy;

public interface ITenantFeature
{
    public TenantInfo Tenant { get; }
}
