using Zad.Modules.Tenancy.Contracts;

namespace Zad.BuildingBlocks.Web.Localization;

public static class CultureResolver
{
    public const string DefaultLanguage = "en";

    public static string Resolve(TenantInfo tenant, string? acceptLanguage)
    {
        var enabled = tenant.EnabledLanguages
            .Select(Normalize)
            .Where(code => code.Length > 0)
            .ToHashSet(StringComparer.Ordinal);

        enabled.Add(DefaultLanguage);

        foreach (var tag in PrimaryTags(acceptLanguage))
        {
            if (enabled.Contains(tag))
            {
                return tag;
            }
        }

        var fallback = Normalize(tenant.DefaultLanguage);

        return enabled.Contains(fallback) ? fallback : DefaultLanguage;
    }

    public static IReadOnlyList<string> PrimaryTags(string? acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return [];
        }

        var tags = new List<(string Tag, double Quality, int Order)>();
        var order = 0;

        foreach (var part in acceptLanguage.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var segments = part.Split(';', StringSplitOptions.TrimEntries);
            var tag = Normalize(segments[0]);

            if (tag.Length == 0 || tag == "*")
            {
                order++;
                continue;
            }

            var quality = 1d;

            foreach (var segment in segments.Skip(1))
            {
                if (segment.StartsWith("q=", StringComparison.OrdinalIgnoreCase) &&
                    double.TryParse(segment[2..], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    quality = parsed;
                }
            }

            tags.Add((tag, quality, order++));
        }

        return tags
            .OrderByDescending(tag => tag.Quality)
            .ThenBy(tag => tag.Order)
            .Select(tag => tag.Tag)
            .ToList();
    }

    private static string Normalize(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return string.Empty;
        }

        var primary = language.Trim().Split('-', StringSplitOptions.TrimEntries)[0];

        return primary.ToLowerInvariant();
    }
}

