using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Messaging;
using Zad.BuildingBlocks.Application.UnitOfWork;
using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Messaging.Outbox;
using Zad.BuildingBlocks.Messaging.Processing;
using Zad.BuildingBlocks.Messaging.Registry;
using Zad.BuildingBlocks.Messaging.Retention;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Messaging.Registration;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        params Assembly[] contractsAssemblies)
    {
        services.AddSingleton(new IntegrationEventRegistry(contractsAssemblies));
        services.AddSingleton<ModuleMessagingRegistry>();
        services.AddSingleton<SubscriptionRegistry>();
        services.AddSingleton<MessagingRetentionService>();
        services.AddSingleton<MessagingRetentionJob>();

        return services;
    }

    public static IServiceCollection AddModuleMessaging<TContext, TUnitOfWork>(
        this IServiceCollection services,
        string module,
        string schema,
        bool processorsEnabled = true)
        where TContext : ModuleDbContext
        where TUnitOfWork : class, IUnitOfWork
    {
        services.AddSingleton(new ModuleMessagingRegistration(module, schema, typeof(TContext), typeof(TUnitOfWork)));
        services.AddScoped<OutboxWriter<TContext>>();

        if (processorsEnabled)
        {
            services.AddHostedService<OutboxProcessor<TContext>>();
            services.AddHostedService<InboxProcessor<TContext>>();
        }

        return services;
    }

    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : class, IIntegrationEvent
        where THandler : class
    {
        services.AddScoped<THandler>();

        var handlerType = typeof(THandler);
        var lane = LaneOf(handlerType);
        var eventName = typeof(TEvent).GetCustomAttribute<IntegrationEventNameAttribute>()?.Name
            ?? throw new InvalidOperationException(
                $"Integration event '{typeof(TEvent).FullName}' is missing [IntegrationEventName]");

        services.AddSingleton(new HandlerDescriptor(
            eventName,
            handlerType.FullName ?? handlerType.Name,
            lane,
            ModuleOf(handlerType.Assembly),
            (provider, @event, effect, cancellationToken) => InvokeAsync<TEvent, THandler>(
                provider, lane, @event, effect, cancellationToken)));

        return services;
    }

    private static Task<Result> InvokeAsync<TEvent, THandler>(
        IServiceProvider provider,
        HandlerLane lane,
        IIntegrationEvent @event,
        ExternalEffectContext? effect,
        CancellationToken cancellationToken)
        where TEvent : class, IIntegrationEvent
        where THandler : class
    {
        var handler = provider.GetRequiredService<THandler>();

        return lane == HandlerLane.Transactional
            ? ((IIntegrationEventHandler<TEvent>)handler).HandleAsync((TEvent)@event, cancellationToken)
            : ((IExternalIntegrationEventHandler<TEvent>)handler).HandleAsync(
                (TEvent)@event,
                effect ?? throw new InvalidOperationException("The external lane requires an effect context"),
                cancellationToken);
    }

    private static HandlerLane LaneOf(Type handlerType)
    {
        var lanes = handlerType.GetInterfaces()
            .Where(type => type.IsGenericType)
            .Select(type => type.GetGenericTypeDefinition())
            .Where(type => type == typeof(IIntegrationEventHandler<>) ||
                           type == typeof(IExternalIntegrationEventHandler<>))
            .Distinct()
            .ToList();

        return lanes.Count == 1
            ? lanes[0] == typeof(IIntegrationEventHandler<>) ? HandlerLane.Transactional : HandlerLane.External
            : throw new InvalidOperationException(
                $"Handler '{handlerType.FullName}' must implement exactly one handler lane (R39)");
    }

    private static string ModuleOf(Assembly assembly)
    {
        var name = assembly.GetName().Name ?? string.Empty;

        const string prefix = "Zad.Modules.";
        const string suffix = ".Infrastructure";

        return name.StartsWith(prefix, StringComparison.Ordinal) &&
               name.EndsWith(suffix, StringComparison.Ordinal)
            ? name[prefix.Length..^suffix.Length]
            : name;
    }
}
