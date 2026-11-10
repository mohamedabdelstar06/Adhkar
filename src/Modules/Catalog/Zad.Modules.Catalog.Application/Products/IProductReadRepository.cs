using Zad.BuildingBlocks.Contracts.Paging;
using Zad.Modules.Catalog.Contracts.Products;

namespace Zad.Modules.Catalog.Application.Products;

public interface IProductReadRepository
{
    public Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<ProductDto>> GetByIdsAsync(IReadOnlyList<Guid> productIds, CancellationToken cancellationToken = default);

    public Task<PagedList<ProductListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
