using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Persistence.UnitOfWork;
using Zad.Modules.Orders.Application;
using Zad.Modules.Orders.Contracts;

namespace Zad.Modules.Orders.Infrastructure.Persistence;

public sealed class OrdersUnitOfWork(OrdersDbContext context)
    : ModuleUnitOfWork<OrdersDbContext>(context), IOrdersUnitOfWork
{
    protected override Error ConcurrencyConflict =>
        Error.Conflict(OrdersErrorCodes.OrderNotFound, "The order was modified concurrently by another process");
}
