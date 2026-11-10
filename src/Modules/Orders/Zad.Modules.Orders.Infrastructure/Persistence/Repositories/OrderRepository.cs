using Microsoft.EntityFrameworkCore;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository(OrdersDbContext context) : IOrderRepository
{
    public Task<Order?> FindAsync(OrderId id, CancellationToken cancellationToken = default) =>
        context.Orders.Include(order => order.Items).FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<Order?> FindByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var normalized = orderNumber.Trim().ToUpperInvariant();

        return context.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.OrderNumber == normalized, cancellationToken);
    }

    public void Add(Order order) => context.Orders.Add(order);

    public void Remove(Order order) => context.Orders.Remove(order);
}
