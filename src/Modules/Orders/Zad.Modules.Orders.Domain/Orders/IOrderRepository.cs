namespace Zad.Modules.Orders.Domain.Orders;

public interface IOrderRepository
{
    public Task<Order?> FindAsync(OrderId id, CancellationToken cancellationToken = default);

    public Task<Order?> FindByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    public void Add(Order order);

    public void Remove(Order order);
}
