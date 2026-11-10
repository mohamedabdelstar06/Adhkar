namespace Zad.BuildingBlocks.Persistence.Migrations;

public sealed record MigrationCurrencyReport(IReadOnlyList<PendingMigration> Pending)
{
    public static readonly MigrationCurrencyReport Current = new([]);

    public bool IsCurrent => Pending.Count == 0;

    public string Describe() =>
        string.Join(
            Environment.NewLine,
            Pending.Select(p => $"  - {p.DatabaseName}: module {p.Module} is missing migration {p.MigrationId}"));
}
