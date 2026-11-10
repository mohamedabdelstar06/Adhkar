using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Application.Messaging;

public interface IIntegrationEventHandler<in TEvent> where TEvent : class, IIntegrationEvent
{
    public Task<Result> HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

