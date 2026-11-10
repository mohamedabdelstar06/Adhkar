namespace Zad.BuildingBlocks.Messaging.Registry;

public sealed class SubscriptionRegistry
{
    private readonly Dictionary<string, List<Subscription>> _byEvent = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Subscription> _byHandler = new(StringComparer.Ordinal);

    public SubscriptionRegistry(
        IEnumerable<HandlerDescriptor> handlers,
        ModuleMessagingRegistry modules)
    {
        foreach (var handler in handlers)
        {
            var module = modules.Find(handler.ConsumerModule)
                ?? throw new InvalidOperationException(
                    $"Handler '{handler.HandlerName}' belongs to module '{handler.ConsumerModule}', " +
                    "which never called AddModuleMessaging");

            var subscription = new Subscription(
                handler.EventName,
                handler.HandlerName,
                handler.Lane,
                handler.ConsumerModule,
                module.Schema,
                module.ContextType,
                module.UnitOfWorkType,
                MessagingContextFactory.For(module.ContextType),
                handler.Invoke);

            if (!_byHandler.TryAdd(subscription.HandlerName, subscription))
            {
                throw new InvalidOperationException($"Handler name '{subscription.HandlerName}' is registered twice");
            }

            if (!_byEvent.TryGetValue(subscription.EventName, out var list))
            {
                list = [];
                _byEvent[subscription.EventName] = list;
            }

            list.Add(subscription);
        }
    }

    public IReadOnlyList<Subscription> For(string eventName) =>
        _byEvent.TryGetValue(eventName, out var list) ? list : [];

    public Subscription? ForHandler(string handlerName) =>
        _byHandler.GetValueOrDefault(handlerName);
}
