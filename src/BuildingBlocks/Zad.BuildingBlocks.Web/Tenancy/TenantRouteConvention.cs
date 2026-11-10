using Microsoft.AspNetCore.Http;

namespace Zad.BuildingBlocks.Web.Tenancy;

public static class TenantRouteConvention
{
    private static readonly string[] TenantlessPrefixes =
    [
        "/health",
        "/openapi",
        "/scalar",
        "/ops/jobs"
    ];

    public static bool IsTenantless(PathString path)
    {
        var value = path.Value;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return TenantlessPrefixes.Any(prefix =>
            value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            (value.Length == prefix.Length || value[prefix.Length] == '/'));
    }
}
