
namespace Zad.Modules.Tenancy.Domain.Databases;

public interface ITenantDatabaseRepository
{
    public Task<TenantDatabase?> FindAsync(DatabaseId id, CancellationToken cancellationToken = default);

    public Task<TenantDatabase?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    public Task<TenantDatabase?> FindOnlineSharedAsync(CancellationToken cancellationToken = default);

    public void Add(TenantDatabase database);
}
