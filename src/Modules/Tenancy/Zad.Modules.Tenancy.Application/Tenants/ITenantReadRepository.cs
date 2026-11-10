using Zad.Modules.Tenancy.Contracts;

namespace Zad.Modules.Tenancy.Application.Tenants;

public interface ITenantReadRepository
{
    public Task<TenantInfo?> ResolveByHostAsync(string host, CancellationToken cancellationToken = default);

    public Task<TenantInfo?> ResolveBySlugAsync(string slug, CancellationToken cancellationToken = default);

    public Task<TenantInfo?> GetAsync(Guid tenantId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<TenantInfo>> ListActiveAsync(CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<TenantRegistrationDto>> ListAllAsync(CancellationToken cancellationToken = default);
}
