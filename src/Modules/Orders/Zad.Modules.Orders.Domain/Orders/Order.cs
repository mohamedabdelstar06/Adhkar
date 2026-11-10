using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Entities;

namespace Zad.Modules.Orders.Domain.Orders;

public sealed class Order : TenantScopedAggregateRoot<OrderId>
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public string OrderNumber { get; private set; } = default!;
    public string CustomerEmail { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public static Result<Order> Create(string orderNumber, string customerEmail) =>
        Result<Order>.Success(new Order
        {
            Id = OrderId.New(),
            OrderNumber = orderNumber.Trim().ToUpperInvariant(),
            CustomerEmail = customerEmail.Trim().ToLowerInvariant(),
            Status = OrderStatus.Draft,
            TotalAmount = 0
        });

    public Result AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(OrderErrors.InvalidStatus);
        }

        var item = OrderItem.Create(productId, productName, unitPrice, quantity);
        _items.Add(item);
        TotalAmount += item.LineTotal;

        return Result.Success();
    }

    public Result Place()
    {
        if (_items.Count == 0)
        {
            return Result.Failure(OrderErrors.EmptyOrder);
        }

        Status = OrderStatus.Placed;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is not (OrderStatus.Placed or OrderStatus.Draft))
        {
            return Result.Failure(OrderErrors.CannotCancel);
        }

        Status = OrderStatus.Cancelled;
        return Result.Success();
    }
}
