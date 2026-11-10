using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.Contexts;
using Zad.BuildingBlocks.Messaging;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Infrastructure.Persistence;

[ModuleDatabase(ModuleDatabaseKind.Tenant, SchemaName)]
public sealed class CatalogDbContext(
    DbContextOptions<CatalogDbContext> options,
    ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    public const string SchemaName = "catalog";

    protected override string Schema => SchemaName;

    public DbSet<Product> Products => Set<Product>();

    protected override void ConfigureEngineTables(ModelBuilder modelBuilder)
    {
        modelBuilder.AddMessagingTables();
    }
}
