using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Entities;

namespace Zad.Modules.Catalog.Domain.Products;

public sealed class Product : TenantScopedAggregateRoot<ProductId>
{
    private Product()
    {
    }

    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    public static Result<Product> Create(
        string sku,
        string name,
        string description,
        decimal unitPrice,
        int stockQuantity)
    {
        if (unitPrice <= 0)
        {
            return Result<Product>.Failure(ProductErrors.InvalidPrice);
        }

        return Result<Product>.Success(new Product
        {
            Id = ProductId.New(),
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            UnitPrice = unitPrice,
            StockQuantity = Math.Max(0, stockQuantity),
            IsActive = true
        });
    }

    public Result UpdateDetails(string name, string description, decimal unitPrice)
    {
        if (unitPrice <= 0)
        {
            return Result.Failure(ProductErrors.InvalidPrice);
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        UnitPrice = unitPrice;

        return Result.Success();
    }

    public Result AdjustStock(int delta)
    {
        if (StockQuantity + delta < 0)
        {
            return Result.Failure(ProductErrors.InsufficientStock);
        }

        StockQuantity += delta;
        return Result.Success();
    }

    public Result Deactivate()
    {
        IsActive = false;
        return Result.Success();
    }
}
