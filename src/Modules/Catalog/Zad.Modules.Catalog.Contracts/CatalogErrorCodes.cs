namespace Zad.Modules.Catalog.Contracts;

public static class CatalogErrorCodes
{
    public const string ProductNotFound = "catalog.product-not-found";
    public const string DuplicateSku = "catalog.duplicate-sku";
    public const string InsufficientStock = "catalog.insufficient-stock";
    public const string InvalidPrice = "catalog.invalid-price";
}
