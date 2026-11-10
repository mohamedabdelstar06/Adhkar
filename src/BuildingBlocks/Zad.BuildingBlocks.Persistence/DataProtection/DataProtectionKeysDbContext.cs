using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Persistence.DataProtection;

[ModuleDatabase(ModuleDatabaseKind.Catalog, SchemaName)]
public sealed class DataProtectionKeysDbContext(
    DbContextOptions<DataProtectionKeysDbContext> options,
    ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext), IDataProtectionKeyContext
{
    public const string SchemaName = "dataprotection";

    protected override string Schema => SchemaName;

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void ConfigureEngineTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataProtectionKey>(builder =>
        {
            builder.ToTable("Keys", SchemaName);
            builder.HasKey(key => key.Id);
        });
    }
}
