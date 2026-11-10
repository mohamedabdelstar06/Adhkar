using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.Contexts;
using Zad.BuildingBlocks.Messaging;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Infrastructure.Persistence;

[ModuleDatabase(ModuleDatabaseKind.Tenant, SchemaName)]
public sealed class OrdersDbContext(
    DbContextOptions<OrdersDbContext> options,
    ITenantContext tenantContext)
    : ModuleDbContext(options, tenantContext)
{
    public const string SchemaName = "orders";

    protected override string Schema => SchemaName;

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void ConfigureEngineTables(ModelBuilder modelBuilder)
    {
        modelBuilder.AddMessagingTables();
    }
}
