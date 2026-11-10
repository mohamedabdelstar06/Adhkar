namespace Zad.BuildingBlocks.Persistence.Contexts;

public interface IModuleDbContextFactory<TContext> where TContext : ModuleDbContext
{
    public TContext CreateForDatabase(Guid databaseId);

    public TContext CreateForCatalog();
}
