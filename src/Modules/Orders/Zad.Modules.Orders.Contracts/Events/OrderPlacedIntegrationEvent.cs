using Zad.BuildingBlocks.Contracts.Messaging;

namespace Zad.Modules.Orders.Contracts.Events;

[IntegrationEventName("orders.order-placed.v1")]
public sealed record OrderPlacedIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    decimal TotalAmount,
    IReadOnlyList<OrderPlacedItemDto> Items) : IIntegrationEvent;
