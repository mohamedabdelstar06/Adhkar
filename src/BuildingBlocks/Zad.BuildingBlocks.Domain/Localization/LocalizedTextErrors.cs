using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Domain.Localization;

public static class LocalizedTextErrors
{
    public static readonly Error EnglishRequired =
        Error.Validation("localization.english-required", "English translation is required");
}

