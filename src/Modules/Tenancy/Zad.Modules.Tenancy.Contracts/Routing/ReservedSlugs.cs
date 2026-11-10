using System.Collections.Frozen;

namespace Zad.Modules.Tenancy.Contracts.Routing;

public static class ReservedSlugs
{
    public static readonly FrozenSet<string> All = FrozenSet.ToFrozenSet(
    [
        "www", "admin", "api", "app", "static", "assets", "cdn", "mail", "smtp", "status",
        "docs", "help", "support", "auth", "login", "id", "test", "dev", "staging", "production",
    ], StringComparer.Ordinal);

    public static bool Contains(string slug) => All.Contains(slug);
}
