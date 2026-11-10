using Zad.BuildingBlocks.Contracts.Messaging;

namespace Zad.Modules.Catalog.Contracts.Events;

[IntegrationEventName("catalog.product-created.v1")]
public sealed record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string Sku,
    string Name,
    decimal UnitPrice) : IIntegrationEvent;

