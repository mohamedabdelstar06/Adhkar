using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.DesignTime;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence;

public sealed class TenancyDbContextFactory : DesignTimeDbContextFactoryBase<TenancyDbContext>
{
    protected override TenancyDbContext Create(DbContextOptions<TenancyDbContext> options) =>
        new(options, UnresolvedTenantContext.Instance);
}
