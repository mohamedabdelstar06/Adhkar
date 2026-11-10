namespace Zad.Modules.Orders.Contracts.Events;

public sealed record OrderPlacedItemDto(Guid ProductId, int Quantity, decimal UnitPrice);

