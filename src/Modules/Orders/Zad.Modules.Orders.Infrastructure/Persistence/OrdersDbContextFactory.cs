using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.DesignTime;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.Modules.Orders.Infrastructure.Persistence;

public sealed class OrdersDbContextFactory : DesignTimeDbContextFactoryBase<OrdersDbContext>
{
    protected override OrdersDbContext Create(DbContextOptions<OrdersDbContext> options) =>
        new(options, UnresolvedTenantContext.Instance);
}
