using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Domain.Entities;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot where TId : struct, IEntityId
{
    public byte[] RowVersion { get; private set; } = [];
}

