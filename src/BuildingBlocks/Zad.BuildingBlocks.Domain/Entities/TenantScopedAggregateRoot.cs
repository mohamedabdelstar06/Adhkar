using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Domain.Entities;

public abstract class TenantScopedAggregateRoot<TId> : TenantScopedEntity<TId>, IAggregateRoot where TId : struct, IEntityId
{
    public byte[] RowVersion { get; private set; } = [];
}

