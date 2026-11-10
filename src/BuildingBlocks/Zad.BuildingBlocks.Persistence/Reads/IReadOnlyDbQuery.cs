using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Persistence.Reads;

public interface IReadOnlyDbQuery<TContext> where TContext : ModuleDbContext
{
    public IQueryable<TEntity> Query<TEntity>() where TEntity : class;
}

