using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Persistence.Reads;

public sealed class ReadOnlyDbQuery<TContext>(TContext context) : IReadOnlyDbQuery<TContext>
    where TContext : ModuleDbContext
{
    public IQueryable<TEntity> Query<TEntity>() where TEntity : class =>
        context.Set<TEntity>().AsNoTracking();
}

