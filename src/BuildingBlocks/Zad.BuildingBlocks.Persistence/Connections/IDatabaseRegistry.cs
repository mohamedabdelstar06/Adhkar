namespace Zad.BuildingBlocks.Persistence.Connections;

public interface IDatabaseRegistry
{
    public IReadOnlyList<DatabaseDescriptor> List();

    public DatabaseDescriptor? Find(Guid databaseId);
}
