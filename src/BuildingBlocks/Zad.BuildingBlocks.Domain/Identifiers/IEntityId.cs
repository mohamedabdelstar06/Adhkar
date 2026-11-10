namespace Zad.BuildingBlocks.Domain.Identifiers;

public interface IEntityId
{
    public Guid Value { get; }
}

public interface IEntityId<TSelf> : IEntityId, IParsable<TSelf> where TSelf : struct, IEntityId<TSelf>
{
    public static abstract TSelf New();

    public static abstract TSelf From(Guid value);
}
