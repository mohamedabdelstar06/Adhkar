using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.Modules.Adhkar.Domain.Adhkar;

public static class AdhkarErrors
{
    public static readonly Error NotFound =
        Error.NotFound("adhkar.not-found", "The requested dhikr does not exist");

    public static readonly Error TextRequired =
        Error.Validation("adhkar.text-required", "The dhikr text is required");

    public static readonly Error InvalidCategory =
        Error.Validation("adhkar.invalid-category", "The category is not one of the supported adhkar categories");

    public static readonly Error DuplicateOrder =
        Error.Conflict("adhkar.duplicate-order", "Another dhikr already uses this display order");

    public static readonly Error NothingToRotate =
        Error.Unprocessable("adhkar.nothing-to-rotate", "There are no active adhkar to rotate");
}
