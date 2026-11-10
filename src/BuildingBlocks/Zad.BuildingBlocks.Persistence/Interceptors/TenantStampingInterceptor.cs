using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Domain.Tenancy;

namespace Zad.BuildingBlocks.Persistence.Interceptors;

public sealed class TenantStampingInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        StampTenant(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampTenant(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampTenant(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<ITenantScoped>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        if (!tenantContext.IsResolved)
        {
            throw new InvalidOperationException("Tenant context must be resolved before writing tenant-scoped entities");
        }

        var tenantId = tenantContext.TenantId;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(ITenantScoped.TenantId)).CurrentValue = tenantId;
            }
            else if (entry.Entity.TenantId != tenantId)
            {
                throw new InvalidOperationException("Cross-tenant modification detected and blocked");
            }
        }
    }
}

