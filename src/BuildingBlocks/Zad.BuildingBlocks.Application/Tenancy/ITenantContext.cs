namespace Zad.BuildingBlocks.Application.Tenancy;

public interface ITenantContext
{
    public bool IsResolved { get; }
    public Guid TenantId { get; }
}
