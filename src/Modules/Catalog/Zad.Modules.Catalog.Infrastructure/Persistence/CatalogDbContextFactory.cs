using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.DesignTime;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContextFactory : DesignTimeDbContextFactoryBase<CatalogDbContext>
{
    protected override CatalogDbContext Create(DbContextOptions<CatalogDbContext> options) =>
        new(options, UnresolvedTenantContext.Instance);
}
