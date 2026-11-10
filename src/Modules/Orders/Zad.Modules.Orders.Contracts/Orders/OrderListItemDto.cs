namespace Zad.Modules.Orders.Contracts.Orders;

public sealed record OrderListItemDto(
    Guid Id,
    string OrderNumber,
    string CustomerEmail,
    decimal TotalAmount,
    string Status,
    int ItemCount,
    DateTimeOffset CreatedAtUtc);

