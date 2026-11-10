using Zad.BuildingBlocks.Contracts.Messaging;

namespace Zad.BuildingBlocks.Application.Messaging;

public interface IOutboxWriter
{
    public void Add<TEvent>(TEvent @event) where TEvent : class, IIntegrationEvent;
}

