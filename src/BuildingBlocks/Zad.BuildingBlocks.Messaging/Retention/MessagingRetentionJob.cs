namespace Zad.BuildingBlocks.Messaging.Retention;

public sealed class MessagingRetentionJob(MessagingRetentionService retentionService)
{
    public Task RunAsync(CancellationToken cancellationToken) => retentionService.PurgeAsync(cancellationToken);
}
