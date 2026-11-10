using Microsoft.EntityFrameworkCore;
using Zad.Modules.Tenancy.Domain.Databases;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence.Repositories;

public sealed class TenantDatabaseRepository(TenancyDbContext context) : ITenantDatabaseRepository
{
    public Task<TenantDatabase?> FindAsync(DatabaseId id, CancellationToken cancellationToken = default) =>
        context.Databases.FirstOrDefaultAsync(database => database.Id == id, cancellationToken);

    public Task<TenantDatabase?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();
        return context.Databases.FirstOrDefaultAsync(database => database.Name == normalized, cancellationToken);
    }

    public Task<TenantDatabase?> FindOnlineSharedAsync(CancellationToken cancellationToken = default) =>
        context.Databases
            .Where(database => database.Kind == DatabaseKind.Shared && database.Status == DatabaseLifecycle.Online)
            .OrderBy(database => database.Name)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(TenantDatabase database) => context.Databases.Add(database);
}

