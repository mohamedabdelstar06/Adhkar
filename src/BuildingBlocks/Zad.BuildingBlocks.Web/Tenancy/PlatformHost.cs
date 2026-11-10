namespace Zad.BuildingBlocks.Web.Tenancy;

public static class PlatformHost
{
    private const string AdminLabel = "admin";

    public static bool IsPlatformHost(string? host, string? rootDomain)
    {
        var normalizedHost = HostNormalization.Normalize(host);
        var normalizedRoot = HostNormalization.Normalize(rootDomain);

        if (normalizedHost is null || normalizedRoot is null)
        {
            return false;
        }

        return string.Equals(normalizedHost, $"{AdminLabel}.{normalizedRoot}", StringComparison.Ordinal);
    }
}
