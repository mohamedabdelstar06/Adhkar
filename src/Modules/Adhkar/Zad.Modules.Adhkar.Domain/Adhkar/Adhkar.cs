using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Entities;

namespace Zad.Modules.Adhkar.Domain.Adhkar;

public sealed class Adhkar : TenantScopedAggregateRoot<AdhkarId>
{
    private Adhkar()
    {
    }

    public string Text { get; private set; } = default!;
    public string Translation { get; private set; } = default!;
    public string Reference { get; private set; } = default!;
    public string Category { get; private set; } = default!;
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }

    public static Result<Adhkar> Create(
        string text,
        string translation,
        string reference,
        string category,
        int displayOrder,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<Adhkar>.Failure(AdhkarErrors.TextRequired);
        }

        var normalizedCategory = category.Trim().ToLowerInvariant();
        if (!AdhkarCategories.IsValid(normalizedCategory))
        {
            return Result<Adhkar>.Failure(AdhkarErrors.InvalidCategory);
        }

        return Result<Adhkar>.Success(new Adhkar
        {
            Id = AdhkarId.New(),
            Text = text.Trim(),
            Translation = translation?.Trim() ?? string.Empty,
            Reference = reference?.Trim() ?? string.Empty,
            Category = normalizedCategory,
            DisplayOrder = displayOrder,
            IsActive = isActive
        });
    }

    public Result Update(string text, string translation, string reference, string category, int displayOrder, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure(AdhkarErrors.TextRequired);
        }

        var normalizedCategory = category.Trim().ToLowerInvariant();
        if (!AdhkarCategories.IsValid(normalizedCategory))
        {
            return Result.Failure(AdhkarErrors.InvalidCategory);
        }

        Text = text.Trim();
        Translation = translation?.Trim() ?? string.Empty;
        Reference = reference?.Trim() ?? string.Empty;
        Category = normalizedCategory;
        DisplayOrder = displayOrder;
        IsActive = isActive;

        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }
}
