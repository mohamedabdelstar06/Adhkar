using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.Reads;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Domain.Databases;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence.ReadRepositories;

public sealed class DatabaseReadRepository(IReadOnlyDbQuery<TenancyDbContext> query) : IDatabaseReadRepository
{
    public async Task<IReadOnlyList<TenantDatabaseDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await query.Query<TenantDatabase>()
            .OrderBy(d => d.Name)
            .Select(d => new TenantDatabaseDto(
                d.Id.Value,
                d.Kind.ToString(),
                d.Name,
                d.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<DatabaseConnectionStrings?> GetConnectionStringsAsync(Guid databaseId, CancellationToken cancellationToken = default)
    {
        var target = DatabaseId.From(databaseId);

        return await query.Query<TenantDatabase>()
            .Where(d => d.Id == target)
            .Select(d => new DatabaseConnectionStrings(
                d.Id.Value,
                d.AppConnectionStringProtected,
                d.MigratorConnectionStringProtected))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
