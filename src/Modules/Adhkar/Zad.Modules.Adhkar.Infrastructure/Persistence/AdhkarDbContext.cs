using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.Contexts;
using Zad.BuildingBlocks.Messaging;
using AdhkarEntity = Zad.Modules.Adhkar.Domain.Adhkar.Adhkar;
using AdhkarRotationEntity = Zad.Modules.Adhkar.Domain.Rotation.AdhkarRotation;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence;

[ModuleDatabase(ModuleDatabaseKind.Tenant, SchemaName)]
public sealed class AdhkarDbContext(
    DbContextOptions<AdhkarDbContext> options,
    ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    public const string SchemaName = "adhkar";

    protected override string Schema => SchemaName;

    public DbSet<AdhkarEntity> Adhkar => Set<AdhkarEntity>();

    public DbSet<AdhkarRotationEntity> Rotations => Set<AdhkarRotationEntity>();

    protected override void ConfigureEngineTables(ModelBuilder modelBuilder)
    {
        modelBuilder.AddMessagingTables();
    }
}
