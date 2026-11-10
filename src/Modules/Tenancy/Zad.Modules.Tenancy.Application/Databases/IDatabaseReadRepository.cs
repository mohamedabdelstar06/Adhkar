namespace Zad.Modules.Tenancy.Application.Databases;

public interface IDatabaseReadRepository
{
    public Task<IReadOnlyList<TenantDatabaseDto>> ListAsync(CancellationToken cancellationToken = default);

    public Task<DatabaseConnectionStrings?> GetConnectionStringsAsync(Guid databaseId, CancellationToken cancellationToken = default);
}
