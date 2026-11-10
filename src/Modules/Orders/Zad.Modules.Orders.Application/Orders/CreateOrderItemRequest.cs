namespace Zad.Modules.Orders.Application.Orders;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

