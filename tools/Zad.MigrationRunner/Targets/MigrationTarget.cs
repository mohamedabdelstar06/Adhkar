namespace Zad.MigrationRunner.Targets;

public sealed record MigrationTarget(
    Guid DatabaseId,
    MigrationTargetKind Kind,
    string DatabaseName,
    string ConnectionString);

