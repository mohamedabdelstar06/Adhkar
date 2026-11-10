using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.DesignTime;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.BuildingBlocks.Persistence.DataProtection;

public sealed class DataProtectionKeysDbContextFactory : DesignTimeDbContextFactoryBase<DataProtectionKeysDbContext>
{
    protected override DataProtectionKeysDbContext Create(DbContextOptions<DataProtectionKeysDbContext> options) =>
        new(options, UnresolvedTenantContext.Instance);
}
