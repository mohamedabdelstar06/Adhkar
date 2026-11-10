namespace Zad.BuildingBlocks.Domain.SoftDeletion;

public interface ISoftDeletable
{
    public bool IsDeleted { get; }
    public DateTimeOffset? DeletedAtUtc { get; }
    public Guid? DeletedBy { get; }
}
