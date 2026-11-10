namespace Zad.Modules.Catalog.Application.Products;

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string Description,
    decimal UnitPrice,
    int StockQuantity);

