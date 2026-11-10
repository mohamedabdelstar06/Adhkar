namespace Zad.Modules.Catalog.Contracts.Products;

public sealed record ProductListItemDto(
    Guid Id,
    string Sku,
    string Name,
    decimal UnitPrice,
    int StockQuantity,
    bool IsActive);

