namespace Zad.BuildingBlocks.Messaging.Processing;

public sealed record ClaimedInboxMessage(
    Guid Id,
    Guid MessageId,
    string HandlerName,
    Guid TenantId,
    string EventName,
    string Payload,
    string CorrelationId,
    Guid? CausationId,
    int AttemptCount);

