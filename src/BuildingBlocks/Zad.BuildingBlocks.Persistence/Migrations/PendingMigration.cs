namespace Zad.BuildingBlocks.Persistence.Migrations;

public sealed record PendingMigration(string DatabaseName, string Module, string MigrationId);

