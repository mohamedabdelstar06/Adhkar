using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.Contexts;
using Zad.Modules.Tenancy.Domain.Databases;
using Zad.Modules.Tenancy.Domain.Tenants;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence;

[ModuleDatabase(ModuleDatabaseKind.Catalog, SchemaName)]
public sealed class TenancyDbContext(
    DbContextOptions<TenancyDbContext> options,
    ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    public const string SchemaName = "tenancy";

    protected override string Schema => SchemaName;

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantDatabase> Databases => Set<TenantDatabase>();
}
