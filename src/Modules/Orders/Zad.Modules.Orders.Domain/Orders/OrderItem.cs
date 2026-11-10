using Zad.BuildingBlocks.Domain.Entities;

namespace Zad.Modules.Orders.Domain.Orders;

public sealed class OrderItem : TenantScopedEntity<OrderItemId>
{
    private OrderItem()
    {
    }

    public OrderId OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    internal static OrderItem Create(Guid productId, string productName, decimal unitPrice, int quantity) =>
        new()
        {
            Id = OrderItemId.New(),
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
}

