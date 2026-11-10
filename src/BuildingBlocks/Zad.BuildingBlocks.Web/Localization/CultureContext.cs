using Zad.BuildingBlocks.Application.Localization;

namespace Zad.BuildingBlocks.Web.Localization;

public sealed class CultureContext : ICultureContext, ICultureContextSetter
{
    public string Language { get; private set; } = "en";

    public void Set(string language)
    {
        Language = string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant();
    }
}

