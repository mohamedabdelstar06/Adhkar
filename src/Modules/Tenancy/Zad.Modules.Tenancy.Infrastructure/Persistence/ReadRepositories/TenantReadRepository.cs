using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.Reads;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Contracts;
using Zad.Modules.Tenancy.Domain.Databases;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence.ReadRepositories;

public sealed class TenantReadRepository(IReadOnlyDbQuery<TenancyDbContext> query) : ITenantReadRepository
{
    public async Task<TenantInfo?> ResolveByHostAsync(string host, CancellationToken cancellationToken = default)
    {
        var tenant = await query.Query<Domain.Tenants.Tenant>()
            .Where(t => t.Host == host)
            .Select(t => new { t.Id, t.Slug, t.Host, t.Status, t.DatabaseId, t.DefaultLanguage, t.EnabledLanguages, t.TimeZone })
            .FirstOrDefaultAsync(cancellationToken);

        return tenant is null ? null : await BuildAsync(tenant.Id.Value, cancellationToken);
    }

    public async Task<TenantInfo?> ResolveBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tenantId = await query.Query<Domain.Tenants.Tenant>()
            .Where(t => t.Slug == slug)
            .Select(t => t.Id.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return tenantId == Guid.Empty ? null : await BuildAsync(tenantId, cancellationToken);
    }

    public Task<TenantInfo?> GetAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        BuildAsync(tenantId, cancellationToken);

    public async Task<IReadOnlyList<TenantInfo>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        var ids = await query.Query<Domain.Tenants.Tenant>()
            .Where(t => t.Status == Domain.Tenants.TenantStatus.Active)
            .Select(t => t.Id.Value)
            .ToListAsync(cancellationToken);

        var result = new List<TenantInfo>(ids.Count);

        foreach (var id in ids)
        {
            var tenant = await BuildAsync(id, cancellationToken);
            if (tenant is not null)
            {
                result.Add(tenant);
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<TenantRegistrationDto>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await query.Query<Domain.Tenants.Tenant>()
            .OrderBy(t => t.Slug)
            .Select(t => new TenantRegistrationDto(
                t.Id.Value,
                t.Slug,
                t.Host,
                t.Status.ToString(),
                t.DatabaseId,
                t.DefaultLanguage,
                t.TimeZone))
            .ToListAsync(cancellationToken);
    }

    private async Task<TenantInfo?> BuildAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await query.Query<Domain.Tenants.Tenant>()
            .Where(t => t.Id == Domain.Tenants.TenantId.From(tenantId))
            .Select(t => new
            {
                t.Slug,
                t.Host,
                t.Status,
                t.DatabaseId,
                t.DefaultLanguage,
                t.EnabledLanguages,
                t.TimeZone
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        var kind = await query.Query<TenantDatabase>()
            .Where(d => d.Id == DatabaseId.From(tenant.DatabaseId))
            .Select(d => (DatabaseKind?)d.Kind)
            .FirstOrDefaultAsync(cancellationToken);

        return new TenantInfo(
            tenantId,
            tenant.Slug,
            tenant.Host,
            tenant.Status.ToString(),
            tenant.DatabaseId,
            kind == DatabaseKind.Dedicated ? TenantDatabasePlacement.Dedicated : TenantDatabasePlacement.Shared,
            tenant.DefaultLanguage,
            tenant.EnabledLanguages.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            tenant.TimeZone,
            tenant.Status == Domain.Tenants.TenantStatus.Active);
    }
}
