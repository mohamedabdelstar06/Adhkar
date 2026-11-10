using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Messaging.Outbox;
using Zad.Modules.Catalog.Application.Products;

namespace Zad.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogOutboxWriter(OutboxWriter<CatalogDbContext> writer) : ICatalogOutboxWriter
{
    public void Add<TEvent>(TEvent @event) where TEvent : class, IIntegrationEvent => writer.Add(@event);
}
