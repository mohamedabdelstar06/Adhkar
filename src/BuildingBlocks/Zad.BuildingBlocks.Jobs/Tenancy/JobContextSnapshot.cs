namespace Zad.BuildingBlocks.Jobs.Tenancy;

internal sealed record JobContextSnapshot(Guid? TenantId, string CorrelationId, string Language);

