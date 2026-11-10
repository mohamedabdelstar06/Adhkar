using Zad.BuildingBlocks.Contracts.Paging;
using Zad.Modules.Catalog.Contracts.Products;

namespace Zad.Modules.Catalog.Application.Products;

public sealed class ProductReader(IProductReadRepository readRepository) : IProductReader
{
    public Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
        readRepository.GetByIdAsync(productId, cancellationToken);

    public Task<IReadOnlyList<ProductDto>> GetByIdsAsync(IReadOnlyList<Guid> productIds, CancellationToken cancellationToken = default) =>
        readRepository.GetByIdsAsync(productIds, cancellationToken);

    public Task<PagedList<ProductListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        readRepository.ListAsync(page, pageSize, cancellationToken);
}
