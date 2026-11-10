using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Persistence.Configuration;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : TenantScopedEntityConfiguration<OrderItem, OrderItemId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", OrdersDbContext.SchemaName);

        builder.Property(item => item.Id)
            .HasConversion(id => id.Value, value => OrderItemId.From(value));

        builder.Property(item => item.OrderId)
            .HasConversion(id => id.Value, value => OrderId.From(value));

        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 4);
    }
}

