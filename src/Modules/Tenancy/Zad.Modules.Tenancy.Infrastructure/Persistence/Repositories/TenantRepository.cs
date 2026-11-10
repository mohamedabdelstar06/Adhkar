using Microsoft.EntityFrameworkCore;
using Zad.Modules.Tenancy.Domain.Tenants;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(TenancyDbContext context) : ITenantRepository
{
    public Task<Tenant?> FindAsync(TenantId id, CancellationToken cancellationToken = default) =>
        context.Tenants.FirstOrDefaultAsync(tenant => tenant.Id == id, cancellationToken);

    public Task<Tenant?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        return context.Tenants.FirstOrDefaultAsync(tenant => tenant.Slug == normalized, cancellationToken);
    }

    public Task<Tenant?> FindByHostAsync(string host, CancellationToken cancellationToken = default)
    {
        var normalized = host.Trim().ToLowerInvariant();
        return context.Tenants.FirstOrDefaultAsync(tenant => tenant.Host == normalized, cancellationToken);
    }

    public void Add(Tenant tenant) => context.Tenants.Add(tenant);

    public void Remove(Tenant tenant) => context.Tenants.Remove(tenant);
}
