namespace Zad.BuildingBlocks.Messaging.Registry;

public sealed class UnknownIntegrationEventException(string eventName)
    : Exception($"No integration event is registered under the name '{eventName}'")
{
    public string EventName { get; } = eventName;
}

