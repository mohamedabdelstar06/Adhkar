using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Messaging.Processing;
using Zad.BuildingBlocks.Messaging.Registry;
using Zad.BuildingBlocks.Persistence.Connections;

namespace Zad.BuildingBlocks.Messaging.Retention;

public sealed class MessagingRetentionService(
    IServiceScopeFactory scopeFactory,
    ModuleMessagingRegistry modules,
    IDatabaseRegistry databases,
    TimeProvider timeProvider,
    ILogger<MessagingRetentionService> logger)
{
    private const int BatchSize = 1000;

    public static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(7);

    public async Task PurgeAsync(CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow() - RetentionPeriod;

        foreach (var module in modules.All)
        {
            var create = MessagingContextFactory.For(module.ContextType);

            foreach (var database in databases.List())
            {
                if (database.Status != TenantDatabaseStatus.Online)
                {
                    continue;
                }

                await using var scope = scopeFactory.CreateAsyncScope();
                await using var context = create(scope.ServiceProvider, database.Id);

                var outbox = await PurgeAsync(context, MessagingSql.PurgeOutbox(module.Schema), cutoff, cancellationToken);
                var inbox = await PurgeAsync(context, MessagingSql.PurgeInbox(module.Schema), cutoff, cancellationToken);

                logger.LogInformation(
                    "Purged {Outbox} outbox and {Inbox} inbox rows from {Module} in database {DatabaseId}",
                    outbox,
                    inbox,
                    module.Module,
                    database.Id);
            }
        }
    }

    private static async Task<int> PurgeAsync(
        DbContext context,
        string sql,
        DateTimeOffset cutoff,
        CancellationToken cancellationToken)
    {
        var total = 0;

        while (true)
        {
            var deleted = await context.Database.ExecuteSqlRawAsync(sql, new object[] { cutoff }, cancellationToken);
            total += deleted;

            if (deleted < BatchSize)
            {
                return total;
            }
        }
    }
}
