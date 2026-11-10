namespace Zad.Modules.Tenancy.Contracts;

public interface ITenantDirectory
{
    public Task<TenantInfo?> ResolveByHostAsync(string host, CancellationToken cancellationToken = default);

    public Task<TenantInfo?> ResolveBySlugAsync(string slug, CancellationToken cancellationToken = default);

    public Task<TenantInfo?> GetAsync(Guid tenantId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<TenantInfo>> ListActiveAsync(CancellationToken cancellationToken = default);
}
