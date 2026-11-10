using System.Text.RegularExpressions;

namespace Zad.BuildingBlocks.Messaging.Processing;

public static partial class MessagingSql
{
    public static string ClaimOutbox(string schema) => $$"""
        UPDATE TOP ({0}) [{{Validate(schema)}}].[OutboxMessages] WITH (ROWLOCK, READPAST)
        SET [Status] = 'Pending', [AttemptCount] = [AttemptCount] + 1, [LockedBy] = {1}, [LockedUntilUtc] = {2}
        OUTPUT inserted.[Id], inserted.[TenantId], inserted.[EventName], inserted.[Payload],
               inserted.[CorrelationId], inserted.[CausationId], inserted.[AttemptCount]
        WHERE [Status] = 'Pending' AND [NextAttemptAtUtc] <= {3}
              AND ([LockedUntilUtc] IS NULL OR [LockedUntilUtc] < {3});
        """;

    public static string ClaimInbox(string schema) => $$"""
        UPDATE TOP ({0}) [{{Validate(schema)}}].[InboxMessages] WITH (ROWLOCK, READPAST)
        SET [Status] = 'Pending', [AttemptCount] = [AttemptCount] + 1, [LockedBy] = {1}, [LockedUntilUtc] = {2}
        OUTPUT inserted.[Id], inserted.[MessageId], inserted.[HandlerName], inserted.[TenantId],
               inserted.[EventName], inserted.[Payload], inserted.[CorrelationId], inserted.[CausationId],
               inserted.[AttemptCount]
        WHERE [Status] = 'Pending' AND [NextAttemptAtUtc] <= {3}
              AND ([LockedUntilUtc] IS NULL OR [LockedUntilUtc] < {3});
        """;

    public static string FanOutInbox(string schema) => $$"""
        INSERT INTO [{{Validate(schema)}}].[InboxMessages]
            ([Id], [MessageId], [HandlerName], [TenantId], [EventName], [Payload],
             [CorrelationId], [CausationId], [ReceivedAtUtc], [Status], [AttemptCount], [NextAttemptAtUtc])
        SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, 'Pending', 0, {8}
        WHERE NOT EXISTS (
            SELECT 1 FROM [{{Validate(schema)}}].[InboxMessages] WITH (UPDLOCK, HOLDLOCK)
            WHERE [MessageId] = {1} AND [HandlerName] = {2});
        """;

    public static string MarkOutboxDispatched(string schema) => $$"""
        UPDATE [{{Validate(schema)}}].[OutboxMessages]
        SET [Status] = 'Dispatched', [DispatchedAtUtc] = {0}, [LockedBy] = NULL, [LockedUntilUtc] = NULL
        WHERE [Id] = {1} AND [LockedBy] = {2} AND [Status] = 'Pending';
        """;

    public static string MarkInboxProcessed(string schema) => $$"""
        UPDATE [{{Validate(schema)}}].[InboxMessages]
        SET [Status] = 'Processed', [ProcessedAtUtc] = {0}, [LockedBy] = NULL, [LockedUntilUtc] = NULL
        WHERE [Id] = {1} AND [LockedBy] = {2} AND [Status] = 'Pending';
        """;

    public static string RetryOutbox(string schema) => $$"""
        UPDATE [{{Validate(schema)}}].[OutboxMessages]
        SET [Status] = {0}, [NextAttemptAtUtc] = {1}, [LastError] = {2},
            [LockedBy] = NULL, [LockedUntilUtc] = NULL
        WHERE [Id] = {3} AND [LockedBy] = {4} AND [Status] = 'Pending';
        """;

    public static string RetryInbox(string schema) => $$"""
        UPDATE [{{Validate(schema)}}].[InboxMessages]
        SET [Status] = {0}, [NextAttemptAtUtc] = {1}, [LastError] = {2},
            [LockedBy] = NULL, [LockedUntilUtc] = NULL
        WHERE [Id] = {3} AND [LockedBy] = {4} AND [Status] = 'Pending';
        """;

    public static string PurgeOutbox(string schema) => $$"""
        DELETE TOP (1000) FROM [{{Validate(schema)}}].[OutboxMessages]
        WHERE [Status] = 'Dispatched' AND [DispatchedAtUtc] < {0};
        """;

    public static string PurgeInbox(string schema) => $$"""
        DELETE TOP (1000) FROM [{{Validate(schema)}}].[InboxMessages]
        WHERE [Status] = 'Processed' AND [ProcessedAtUtc] < {0};
        """;

    private static string Validate(string schema) =>
        SchemaPattern().IsMatch(schema)
            ? schema
            : throw new ArgumentException($"'{schema}' is not a valid schema name", nameof(schema));

    [GeneratedRegex("^[a-z][a-z0-9_]*$")]
    private static partial Regex SchemaPattern();
}
