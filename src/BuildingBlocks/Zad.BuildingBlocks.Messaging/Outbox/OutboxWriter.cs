using Zad.BuildingBlocks.Application.Messaging;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Messaging.Registry;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Messaging.Outbox;

public sealed class OutboxWriter<TContext>(
    TContext dbContext,
    IntegrationEventRegistry events,
    ITenantContext tenantContext,
    ICorrelationContext correlationContext,
    TimeProvider timeProvider) : IOutboxWriter
    where TContext : ModuleDbContext
{
    public void Add<TEvent>(TEvent @event) where TEvent : class, IIntegrationEvent
    {
        if (!tenantContext.IsResolved)
        {
            throw new InvalidOperationException("An integration event requires a resolved tenant context");
        }

        var (eventName, payload) = events.Serialize(@event);
        var now = timeProvider.GetUtcNow();

        dbContext.Set<OutboxMessage>().Add(OutboxMessage.Create(
            GuidV7.New(),
            tenantContext.TenantId,
            eventName,
            payload,
            now,
            correlationContext.CorrelationId,
            correlationContext.CausationId));
    }
}
