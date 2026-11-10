using Zad.BuildingBlocks.Messaging.Outbox;

namespace Zad.BuildingBlocks.Messaging.Inbox;

[EngineTable]
public sealed class InboxMessage
{
    private InboxMessage()
    {
    }

    public Guid Id { get; private init; }

    public Guid MessageId { get; private init; }

    public string HandlerName { get; private init; } = default!;

    public Guid TenantId { get; private init; }

    public string EventName { get; private init; } = default!;

    public string Payload { get; private init; } = default!;

    public string CorrelationId { get; private init; } = default!;

    public Guid? CausationId { get; private init; }

    public DateTimeOffset ReceivedAtUtc { get; private init; }

    public MessageStatus Status { get; private set; } = MessageStatus.Pending;

    public int AttemptCount { get; private set; }

    public DateTimeOffset NextAttemptAtUtc { get; private set; }

    public string? LockedBy { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public static InboxMessage Create(
        Guid id,
        Guid messageId,
        string handlerName,
        Guid tenantId,
        string eventName,
        string payload,
        string correlationId,
        Guid? causationId,
        DateTimeOffset receivedAtUtc) =>
        new()
        {
            Id = id,
            MessageId = messageId,
            HandlerName = handlerName,
            TenantId = tenantId,
            EventName = eventName,
            Payload = payload,
            CorrelationId = correlationId,
            CausationId = causationId,
            ReceivedAtUtc = receivedAtUtc,
            Status = MessageStatus.Pending,
            AttemptCount = 0,
            NextAttemptAtUtc = receivedAtUtc
        };
}
