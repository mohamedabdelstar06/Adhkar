using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Messaging.Outbox;
using Zad.Modules.Orders.Application.Orders;

namespace Zad.Modules.Orders.Infrastructure.Persistence;

public sealed class OrdersOutboxWriter(OutboxWriter<OrdersDbContext> writer) : IOrdersOutboxWriter
{
    public void Add<TEvent>(TEvent @event) where TEvent : class, IIntegrationEvent => writer.Add(@event);
}
