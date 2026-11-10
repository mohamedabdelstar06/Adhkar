using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Contracts.Paging;
using Zad.BuildingBlocks.Persistence.Reads;
using Zad.Modules.Catalog.Application.Products;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Infrastructure.Persistence.ReadRepositories;

public sealed class ProductReadRepository(IReadOnlyDbQuery<CatalogDbContext> query) : IProductReadRepository
{
    public async Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var targetId = ProductId.From(productId);

        return await query.Query<Product>()
            .Where(product => product.Id == targetId)
            .Select(product => new ProductDto(
                product.Id.Value,
                product.Sku,
                product.Name,
                product.Description,
                product.UnitPrice,
                product.StockQuantity,
                product.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductDto>> GetByIdsAsync(IReadOnlyList<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var targetIds = productIds.Select(ProductId.From).ToList();

        return await query.Query<Product>()
            .Where(product => targetIds.Contains(product.Id))
            .Select(product => new ProductDto(
                product.Id.Value,
                product.Sku,
                product.Name,
                product.Description,
                product.UnitPrice,
                product.StockQuantity,
                product.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<ProductListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var baseQuery = query.Query<Product>();
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderBy(product => product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new ProductListItemDto(
                product.Id.Value,
                product.Sku,
                product.Name,
                product.UnitPrice,
                product.StockQuantity,
                product.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedList<ProductListItemDto>(items, page, pageSize, totalCount);
    }
}
