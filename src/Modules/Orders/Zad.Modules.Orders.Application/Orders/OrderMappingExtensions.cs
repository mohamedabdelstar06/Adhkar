using Zad.Modules.Orders.Contracts.Orders;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Application.Orders;

internal static class OrderMappingExtensions
{
    internal static OrderDetailsDto ToDetailsDto(this Order order, DateTimeOffset createdAtUtc) => new(
        order.Id.Value,
        order.OrderNumber,
        order.CustomerEmail,
        order.TotalAmount,
        order.Status.ToString(),
        order.Items.Select(item => new OrderItemDto(
            item.Id.Value,
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.LineTotal)).ToList(),
        createdAtUtc);
}

