using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.Modules.Catalog.Domain.Products;

public static class ProductErrors
{
    public static readonly Error NotFound =
        Error.NotFound("catalog.product-not-found", "The requested product was not found");

    public static readonly Error DuplicateSku =
        Error.Conflict("catalog.duplicate-sku", "A product with this SKU already exists");

    public static readonly Error InvalidPrice =
        Error.Validation("catalog.invalid-price", "Unit price must be greater than zero");

    public static readonly Error InsufficientStock =
        Error.Unprocessable("catalog.insufficient-stock", "The product does not have enough stock for this operation");
}
