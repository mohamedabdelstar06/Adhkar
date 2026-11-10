using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Domain.Tenancy;

namespace Zad.BuildingBlocks.Domain.Entities;

public abstract class TenantScopedEntity<TId> : Entity<TId>, ITenantScoped where TId : struct, IEntityId
{
    public Guid TenantId { get; private set; }
}

