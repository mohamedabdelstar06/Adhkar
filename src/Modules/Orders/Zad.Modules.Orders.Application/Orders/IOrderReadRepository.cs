using Zad.BuildingBlocks.Contracts.Paging;
using Zad.Modules.Orders.Contracts.Orders;

namespace Zad.Modules.Orders.Application.Orders;

public interface IOrderReadRepository
{
    public Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    public Task<PagedList<OrderListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
