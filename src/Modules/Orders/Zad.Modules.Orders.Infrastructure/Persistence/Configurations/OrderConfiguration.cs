using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Persistence.Configuration;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : TenantScopedEntityConfiguration<Order, OrderId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", OrdersDbContext.SchemaName);

        builder.Property(order => order.Id)
            .HasConversion(id => id.Value, value => OrderId.From(value));

        builder.Property(order => order.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(order => order.CustomerEmail).HasMaxLength(256).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(order => order.TotalAmount).HasPrecision(18, 4);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => new { item.TenantId, item.OrderId })
            .IsRequired();

        builder.HasIndex(order => new { order.TenantId, order.OrderNumber })
            .IsUnique()
            .HasDatabaseName("UX_Orders_TenantId_OrderNumber");
    }
}

