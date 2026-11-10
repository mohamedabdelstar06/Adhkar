using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Domain.Localization;

public sealed record LocalizedText
{
    public IReadOnlyDictionary<string, string> Translations { get; }

    private LocalizedText(IReadOnlyDictionary<string, string> translations)
    {
        Translations = translations;
    }

    public static Result<LocalizedText> Create(IReadOnlyDictionary<string, string> translations)
    {
        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in translations)
        {
            if (!string.IsNullOrWhiteSpace(pair.Value))
            {
                normalized[pair.Key.Trim().ToLowerInvariant()] = pair.Value.Trim();
            }
        }

        if (!normalized.TryGetValue("en", out var english) || string.IsNullOrWhiteSpace(english))
        {
            return Result<LocalizedText>.Failure(LocalizedTextErrors.EnglishRequired);
        }

        return Result<LocalizedText>.Success(new LocalizedText(normalized));
    }

    public string Resolve(string language)
    {
        var key = language?.Trim().ToLowerInvariant() ?? "en";
        return Translations.TryGetValue(key, out var value) ? value : Translations["en"];
    }
}
