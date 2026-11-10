namespace Zad.Modules.Orders.Contracts.Orders;

public sealed record OrderDetailsDto(
    Guid Id,
    string OrderNumber,
    string CustomerEmail,
    decimal TotalAmount,
    string Status,
    IReadOnlyList<OrderItemDto> Items,
    DateTimeOffset CreatedAtUtc);

