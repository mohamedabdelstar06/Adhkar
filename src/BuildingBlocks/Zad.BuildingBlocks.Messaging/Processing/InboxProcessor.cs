using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Application.Messaging;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Messaging.Outbox;
using Zad.BuildingBlocks.Messaging.Registry;
using Zad.BuildingBlocks.Messaging.Retries;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Messaging.Processing;

public sealed class InboxProcessor<TContext>(
    IServiceScopeFactory scopeFactory,
    IDatabaseRegistry databases,
    IntegrationEventRegistry events,
    SubscriptionRegistry subscriptions,
    TimeProvider timeProvider,
    ILogger<InboxProcessor<TContext>> logger) : BackgroundService
    where TContext : ModuleDbContext
{
    private const int BatchSize = 50;

    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);

    private readonly string _schema = ModuleDbContextDescriptor.For(typeof(TContext)).Schema;
    private readonly string _instanceId = $"{Environment.MachineName}:{Environment.ProcessId}:{GuidV7.New():N}";
    private readonly RetryPolicy _retryPolicy = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await HandleOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Inbox processing for schema {Schema} failed", _schema);
            }

            await Task.Delay(PollingInterval, timeProvider, stoppingToken);
        }
    }

    private async Task HandleOnceAsync(CancellationToken cancellationToken)
    {
        foreach (var database in databases.List())
        {
            if (database.Status != TenantDatabaseStatus.Online)
            {
                continue;
            }

            foreach (var row in await ClaimAsync(database.Id, cancellationToken))
            {
                await HandleRowAsync(row, cancellationToken);
            }
        }
    }

    private async Task<List<ClaimedInboxMessage>> ClaimAsync(Guid databaseId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IModuleDbContextFactory<TContext>>();

        await using var context = factory.CreateForDatabase(databaseId);
        var now = timeProvider.GetUtcNow();

        return await context.Database
            .SqlQueryRaw<ClaimedInboxMessage>(
                MessagingSql.ClaimInbox(_schema),
                BatchSize,
                _instanceId,
                now.Add(LeaseDuration),
                now)
            .ToListAsync(cancellationToken);
    }

    private async Task HandleRowAsync(ClaimedInboxMessage row, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        provider.GetRequiredService<ITenantContextSetter>().Set(row.TenantId);

        var context = provider.GetRequiredService<TContext>();
        var subscription = subscriptions.ForHandler(row.HandlerName);

        if (subscription is null)
        {
            logger.LogError("No handler named {HandlerName} is registered", row.HandlerName);
            await ApplyRetryAsync(context, row, MessagingErrors.HandlerMissing.Message, cancellationToken);
            return;
        }

        IIntegrationEvent @event;

        try
        {
            @event = events.Deserialize(row.EventName, row.Payload);
        }
        catch (Exception exception)
        {
            await ApplyRetryAsync(context, row, RetryPolicy.Describe(exception), cancellationToken);
            return;
        }

        if (subscription.Lane == HandlerLane.Transactional)
        {
            await HandleTransactionalAsync(provider, context, subscription, row, @event, cancellationToken);
            return;
        }

        await HandleExternalAsync(provider, context, subscription, row, @event, cancellationToken);
    }

    private async Task HandleTransactionalAsync(
        IServiceProvider provider,
        TContext context,
        Subscription subscription,
        ClaimedInboxMessage row,
        IIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var unitOfWork = subscription.UnitOfWork(provider);

        Result result;

        try
        {
            result = await unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var handled = await subscription.Invoke(provider, @event, null, token);

                    if (handled.IsFailure)
                    {
                        return handled;
                    }

                    var marked = await context.Database.ExecuteSqlRawAsync(
                        MessagingSql.MarkInboxProcessed(_schema),
                        new object[] { timeProvider.GetUtcNow(), row.Id, _instanceId },
                        token);

                    return marked == 0
                        ? Result.Failure(MessagingErrors.LeaseLost)
                        : await unitOfWork.SaveChangesAsync(token);
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Handler {HandlerName} failed for message {MessageId}",
                subscription.HandlerName,
                row.MessageId);

            await ApplyRetryAsync(context, row, RetryPolicy.Describe(exception), cancellationToken);
            return;
        }

        if (result.IsFailure)
        {
            await ApplyRetryAsync(context, row, result.Error.Code, cancellationToken);
        }
    }

    private async Task HandleExternalAsync(
        IServiceProvider provider,
        TContext context,
        Subscription subscription,
        ClaimedInboxMessage row,
        IIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        Result handled;

        try
        {
            handled = await subscription.Invoke(
                provider,
                @event,
                new ExternalEffectContext(row.MessageId, row.TenantId, row.CorrelationId),
                cancellationToken);
        }
        catch (Exception exception)
        {
            await ApplyRetryAsync(context, row, RetryPolicy.Describe(exception), cancellationToken);
            return;
        }

        if (handled.IsFailure)
        {
            await ApplyRetryAsync(context, row, handled.Error.Code, cancellationToken);
            return;
        }

        await context.Database.ExecuteSqlRawAsync(
            MessagingSql.MarkInboxProcessed(_schema),
            new object[] { timeProvider.GetUtcNow(), row.Id, _instanceId },
            cancellationToken);
    }

    private async Task ApplyRetryAsync(
        TContext context,
        ClaimedInboxMessage row,
        string error,
        CancellationToken cancellationToken)
    {
        var exhausted = _retryPolicy.IsExhausted(row.AttemptCount);
        var now = timeProvider.GetUtcNow();

        await context.Database.ExecuteSqlRawAsync(
            MessagingSql.RetryInbox(_schema),
            new object[]
            {
                exhausted ? nameof(MessageStatus.DeadLettered) : nameof(MessageStatus.Pending),
                exhausted ? now : _retryPolicy.NextAttempt(now, row.AttemptCount),
                error,
                row.Id,
                _instanceId
            },
            cancellationToken);
    }
}
