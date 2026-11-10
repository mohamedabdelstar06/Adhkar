using Zad.Modules.Tenancy.Contracts;

namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed class TenantReader(ITenantReadRepository readRepository) : ITenantDirectory
{
    public Task<TenantInfo?> ResolveByHostAsync(string host, CancellationToken cancellationToken = default) =>
        readRepository.ResolveByHostAsync(host.Trim().ToLowerInvariant(), cancellationToken);

    public Task<TenantInfo?> ResolveBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        readRepository.ResolveBySlugAsync(slug.Trim().ToLowerInvariant(), cancellationToken);

    public Task<TenantInfo?> GetAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        readRepository.GetAsync(tenantId, cancellationToken);

    public Task<IReadOnlyList<TenantInfo>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        readRepository.ListActiveAsync(cancellationToken);
}
