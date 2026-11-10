using System.Reflection;
using System.Text.Json;
using Zad.BuildingBlocks.Contracts.Messaging;

namespace Zad.BuildingBlocks.Messaging.Registry;

public sealed class IntegrationEventRegistry
{
    private readonly Dictionary<string, Type> _byName = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _byType = [];

    public IntegrationEventRegistry(IEnumerable<Assembly> contractsAssemblies)
    {
        foreach (var assembly in contractsAssemblies.Distinct())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface || !typeof(IIntegrationEvent).IsAssignableFrom(type))
                {
                    continue;
                }

                var attribute = type.GetCustomAttribute<IntegrationEventNameAttribute>()
                    ?? throw new InvalidOperationException(
                        $"Integration event '{type.FullName}' is missing [IntegrationEventName]");

                if (!_byName.TryAdd(attribute.Name, type))
                {
                    throw new InvalidOperationException(
                        $"Integration event name '{attribute.Name}' is declared more than once");
                }

                _byType[type] = attribute.Name;
            }
        }
    }

    public IReadOnlyCollection<Type> EventTypes => _byType.Keys;

    public string NameOf(Type eventType) =>
        _byType.TryGetValue(eventType, out var name)
            ? name
            : throw new UnknownIntegrationEventException(eventType.FullName ?? eventType.Name);

    public Type TypeOf(string eventName) =>
        _byName.TryGetValue(eventName, out var type)
            ? type
            : throw new UnknownIntegrationEventException(eventName);

    public (string EventName, string Payload) Serialize(IIntegrationEvent @event)
    {
        var eventName = NameOf(@event.GetType());

        return (eventName, JsonSerializer.Serialize(@event, @event.GetType(), MessagingJson.Options));
    }

    public IIntegrationEvent Deserialize(string eventName, string payload)
    {
        var type = TypeOf(eventName);

        return JsonSerializer.Deserialize(payload, type, MessagingJson.Options) as IIntegrationEvent
            ?? throw new UnknownIntegrationEventException(eventName);
    }
}
