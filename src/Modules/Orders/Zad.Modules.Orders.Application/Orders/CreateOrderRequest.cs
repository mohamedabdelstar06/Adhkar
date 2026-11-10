namespace Zad.Modules.Orders.Application.Orders;

public sealed record CreateOrderRequest(string CustomerEmail, IReadOnlyList<CreateOrderItemRequest> Items);

