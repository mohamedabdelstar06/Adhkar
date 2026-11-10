namespace Zad.BuildingBlocks.Web.Tenancy;

public static class HostNormalization
{
    public static string? Normalize(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var value = host.Trim().ToLowerInvariant();

        if (value.EndsWith('.'))
        {
            value = value[..^1];
        }

        if (value.Length == 0 || !value.All(char.IsAscii))
        {
            return null;
        }

        return value;
    }

    public static string? SingleLabelUnder(string? host, string? rootDomain)
    {
        var normalizedHost = Normalize(host);
        var normalizedRoot = Normalize(rootDomain);

        if (normalizedHost is null || normalizedRoot is null)
        {
            return null;
        }

        var suffix = "." + normalizedRoot;

        if (!normalizedHost.EndsWith(suffix, StringComparison.Ordinal))
        {
            return null;
        }

        var label = normalizedHost[..^suffix.Length];

        return label.Length == 0 || label.Contains('.', StringComparison.Ordinal) ? null : label;
    }
}
