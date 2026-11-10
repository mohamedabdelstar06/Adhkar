namespace Zad.Modules.Tenancy.Domain.Tenants;

public interface ITenantRepository
{
    public Task<Tenant?> FindAsync(TenantId id, CancellationToken cancellationToken = default);

    public Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default);

    public Task<Tenant?> FindByHostAsync(string host, CancellationToken cancellationToken = default);

    public void Add(Tenant tenant);

    public void Remove(Tenant tenant);
}
