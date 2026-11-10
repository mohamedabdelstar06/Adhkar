namespace Zad.BuildingBlocks.Application.Messaging;

public sealed record ExternalEffectContext(Guid MessageId, Guid TenantId, string CorrelationId);

