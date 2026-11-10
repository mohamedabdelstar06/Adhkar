using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Application.Products;

internal static class ProductMappingExtensions
{
    internal static ProductDto ToDto(this Product product) => new(
        product.Id.Value,
        product.Sku,
        product.Name,
        product.Description,
        product.UnitPrice,
        product.StockQuantity,
        product.IsActive);
}

