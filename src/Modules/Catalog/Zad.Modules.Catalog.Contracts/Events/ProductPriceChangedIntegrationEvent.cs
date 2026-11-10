using Zad.BuildingBlocks.Contracts.Messaging;

namespace Zad.Modules.Catalog.Contracts.Events;

[IntegrationEventName("catalog.product-price-changed.v1")]
public sealed record ProductPriceChangedIntegrationEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice) : IIntegrationEvent;

