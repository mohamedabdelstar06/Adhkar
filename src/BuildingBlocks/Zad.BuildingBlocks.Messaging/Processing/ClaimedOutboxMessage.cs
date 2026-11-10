namespace Zad.BuildingBlocks.Messaging.Processing;

public sealed record ClaimedOutboxMessage(
    Guid Id,
    Guid TenantId,
    string EventName,
    string Payload,
    string CorrelationId,
    Guid? CausationId,
    int AttemptCount);

