namespace Zad.BuildingBlocks.Application.Tenancy;

public interface ITenantContextSetter
{
    public void Set(Guid tenantId);
}

