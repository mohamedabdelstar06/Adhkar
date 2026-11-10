namespace Zad.MigrationRunner.Discovery;

public static class ModuleMigrationOrder
{
    public static readonly IReadOnlyList<string> Modules =
    [
        "Persistence",
        "Tenancy",
        "Catalog",
        "Orders",
        "Adhkar"
    ];
}
