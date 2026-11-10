using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.DesignTime;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence;

public sealed class AdhkarDbContextFactory : DesignTimeDbContextFactoryBase<AdhkarDbContext>
{
    protected override AdhkarDbContext Create(DbContextOptions<AdhkarDbContext> options) =>
        new(options, UnresolvedTenantContext.Instance);
}
