namespace Zad.BuildingBlocks.Messaging.Outbox;

[EngineTable]
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private init; }

    public Guid TenantId { get; private init; }

    public string EventName { get; private init; } = default!;

    public string Payload { get; private init; } = default!;

    public DateTimeOffset OccurredAtUtc { get; private init; }

    public string CorrelationId { get; private init; } = default!;

    public Guid? CausationId { get; private init; }

    public MessageStatus Status { get; private set; } = MessageStatus.Pending;

    public int AttemptCount { get; private set; }

    public DateTimeOffset NextAttemptAtUtc { get; private set; }

    public string? LockedBy { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? DispatchedAtUtc { get; private set; }

    public static OutboxMessage Create(
        Guid id,
        Guid tenantId,
        string eventName,
        string payload,
        DateTimeOffset occurredAtUtc,
        string correlationId,
        Guid? causationId) =>
        new()
        {
            Id = id,
            TenantId = tenantId,
            EventName = eventName,
            Payload = payload,
            OccurredAtUtc = occurredAtUtc,
            CorrelationId = correlationId,
            CausationId = causationId,
            Status = MessageStatus.Pending,
            AttemptCount = 0,
            NextAttemptAtUtc = occurredAtUtc
        };
}
