using Zad.BuildingBlocks.Contracts.Paging;

namespace Zad.Modules.Catalog.Contracts.Products;

public interface IProductReader
{
    public Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<ProductDto>> GetByIdsAsync(IReadOnlyList<Guid> productIds, CancellationToken cancellationToken = default);

    public Task<PagedList<ProductListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
