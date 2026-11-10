using Zad.BuildingBlocks.Contracts.Paging;
using Zad.Modules.Orders.Contracts.Orders;

namespace Zad.Modules.Orders.Application.Orders;

public sealed class OrderReader(IOrderReadRepository readRepository) : IOrderReader
{
    public Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        readRepository.GetByIdAsync(orderId, cancellationToken);

    public Task<PagedList<OrderListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        readRepository.ListAsync(page, pageSize, cancellationToken);
}
