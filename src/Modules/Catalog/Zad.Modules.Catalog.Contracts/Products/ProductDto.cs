namespace Zad.Modules.Catalog.Contracts.Products;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string Description,
    decimal UnitPrice,
    int StockQuantity,
    bool IsActive);

