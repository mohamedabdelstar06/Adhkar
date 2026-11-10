using Zad.BuildingBlocks.Contracts.Paging;

namespace Zad.Modules.Orders.Contracts.Orders;

public interface IOrderReader
{
    public Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    public Task<PagedList<OrderListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
