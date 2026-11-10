using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Domain.Entities;

public abstract class Entity<TId> where TId : struct, IEntityId
{
    public TId Id { get; protected init; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset? ModifiedAtUtc { get; private set; }

    public Guid? ModifiedBy { get; private set; }
}
