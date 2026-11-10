namespace Zad.BuildingBlocks.Domain.Tenancy;

public interface ITenantScoped
{
    public Guid TenantId { get; }
}
