using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Application.Messaging;

public interface IExternalIntegrationEventHandler<in TEvent> where TEvent : class, IIntegrationEvent
{
    public Task<Result> HandleAsync(TEvent @event, ExternalEffectContext context, CancellationToken cancellationToken = default);
}

