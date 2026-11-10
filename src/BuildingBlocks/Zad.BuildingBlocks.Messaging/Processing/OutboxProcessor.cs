using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Messaging.Outbox;
using Zad.BuildingBlocks.Messaging.Registry;
using Zad.BuildingBlocks.Messaging.Retries;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Messaging.Processing;

public sealed class OutboxProcessor<TContext>(
    IServiceScopeFactory scopeFactory,
    IDatabaseRegistry databases,
    SubscriptionRegistry subscriptions,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor<TContext>> logger) : BackgroundService
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
                await DispatchOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox dispatch for schema {Schema} failed", _schema);
            }

            await Task.Delay(PollingInterval, timeProvider, stoppingToken);
        }
    }

    private async Task DispatchOnceAsync(CancellationToken cancellationToken)
    {
        foreach (var database in databases.List())
        {
            if (database.Status != TenantDatabaseStatus.Online)
            {
                continue;
            }

            await DispatchDatabaseAsync(database.Id, cancellationToken);
        }
    }

    private async Task DispatchDatabaseAsync(Guid databaseId, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IModuleDbContextFactory<TContext>>();

        await using var context = factory.CreateForDatabase(databaseId);
        var now = timeProvider.GetUtcNow();

        var claimed = await context.Database
            .SqlQueryRaw<ClaimedOutboxMessage>(
                MessagingSql.ClaimOutbox(_schema),
                BatchSize,
                _instanceId,
                now.Add(LeaseDuration),
                now)
            .ToListAsync(cancellationToken);

        foreach (var message in claimed)
        {
            await FanOutAsync(scope.ServiceProvider, context, databaseId, message, cancellationToken);
        }
    }

    private async Task FanOutAsync(
        IServiceProvider provider,
        TContext context,
        Guid databaseId,
        ClaimedOutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            var now = timeProvider.GetUtcNow();

            foreach (var subscription in subscriptions.For(message.EventName))
            {
                await InsertInboxAsync(provider, context, databaseId, subscription, message, now, cancellationToken);
            }

            await context.Database.ExecuteSqlRawAsync(
                MessagingSql.MarkOutboxDispatched(_schema),
                new object[] { now, message.Id, _instanceId },
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Fan-out of {EventName} ({MessageId}) failed",
                message.EventName,
                message.Id);

            await ApplyRetryAsync(context, message, exception, cancellationToken);
        }
    }

    private static async Task InsertInboxAsync(
        IServiceProvider provider,
        TContext context,
        Guid databaseId,
        Subscription subscription,
        ClaimedOutboxMessage message,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var consumer = subscription.ContextType == typeof(TContext)
            ? (DbContext)context
            : subscription.CreateContext(provider, databaseId);

        try
        {
            await consumer.Database.ExecuteSqlRawAsync(
                MessagingSql.FanOutInbox(subscription.Schema),
                new object[]
                {
                    GuidV7.New(),
                    message.Id,
                    subscription.HandlerName,
                    message.TenantId,
                    message.EventName,
                    message.Payload,
                    message.CorrelationId,
                    message.CausationId ?? message.Id,
                    now
                },
                cancellationToken);
        }
        finally
        {
            if (!ReferenceEquals(consumer, context))
            {
                await consumer.DisposeAsync();
            }
        }
    }

    private async Task ApplyRetryAsync(
        TContext context,
        ClaimedOutboxMessage message,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var exhausted = _retryPolicy.IsExhausted(message.AttemptCount);
        var now = timeProvider.GetUtcNow();

        await context.Database.ExecuteSqlRawAsync(
            MessagingSql.RetryOutbox(_schema),
            new object[]
            {
                exhausted ? nameof(MessageStatus.DeadLettered) : nameof(MessageStatus.Pending),
                exhausted ? now : _retryPolicy.NextAttempt(now, message.AttemptCount),
                RetryPolicy.Describe(exception),
                message.Id,
                _instanceId
            },
            cancellationToken);
    }
}
