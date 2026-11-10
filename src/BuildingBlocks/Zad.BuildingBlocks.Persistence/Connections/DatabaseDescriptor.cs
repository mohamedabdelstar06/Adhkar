namespace Zad.BuildingBlocks.Persistence.Connections;

public sealed record DatabaseDescriptor(
    Guid Id,
    TenantDatabaseKind Kind,
    string Name,
    TenantDatabaseStatus Status);
