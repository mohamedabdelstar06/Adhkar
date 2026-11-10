using Zad.BuildingBlocks.Application.Localization;

namespace Zad.MigrationRunner.Seeding;

public sealed class RunnerCultureContext : ICultureContext, ICultureContextSetter
{
    public string Language { get; private set; } = "en";

    public void Set(string language)
    {
        Language = string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant();
    }
}

