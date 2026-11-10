using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Contracts.Paging;
using Zad.BuildingBlocks.Persistence.Reads;
using Zad.Modules.Orders.Application.Orders;
using Zad.Modules.Orders.Contracts.Orders;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Infrastructure.Persistence.ReadRepositories;

public sealed class OrderReadRepository(IReadOnlyDbQuery<OrdersDbContext> query) : IOrderReadRepository
{
    public async Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var targetId = OrderId.From(orderId);

        return await query.Query<Order>()
            .Where(order => order.Id == targetId)
            .Select(order => new OrderDetailsDto(
                order.Id.Value,
                order.OrderNumber,
                order.CustomerEmail,
                order.TotalAmount,
                order.Status.ToString(),
                order.Items.Select(item => new OrderItemDto(
                    item.Id.Value,
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity,
                    item.LineTotal)).ToList(),
                order.CreatedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedList<OrderListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var baseQuery = query.Query<Order>();
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(order => order.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderListItemDto(
                order.Id.Value,
                order.OrderNumber,
                order.CustomerEmail,
                order.TotalAmount,
                order.Status.ToString(),
                order.Items.Count,
                order.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedList<OrderListItemDto>(items, page, pageSize, totalCount);
    }
}
