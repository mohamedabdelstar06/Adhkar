using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Orders.Contracts.Events;
using Zad.Modules.Orders.Contracts.Orders;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.Application.Orders;

public sealed class OrderPlacementService(
    IOrderRepository orderRepository,
    IProductReader productReader,
    IOrdersUnitOfWork unitOfWork,
    IOrdersOutboxWriter outboxWriter,
    TimeProvider timeProvider)
{
    public async Task<Result<OrderDetailsDto>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var orderNumber = $"ORD-{now:yyyyMMddHHmmss}-{now.ToUnixTimeMilliseconds() % 10000}";

        var orderResult = Order.Create(orderNumber, request.CustomerEmail);
        if (orderResult.IsFailure)
        {
            return Result<OrderDetailsDto>.Failure(orderResult.Error);
        }

        var order = orderResult.Value;
        var eventItems = new List<OrderPlacedItemDto>();

        foreach (var requestedItem in request.Items)
        {
            var product = await productReader.GetByIdAsync(requestedItem.ProductId, cancellationToken);
            if (product is null)
            {
                return Result<OrderDetailsDto>.Failure(OrderErrors.ProductNotFound);
            }

            if (!product.IsActive)
            {
                return Result<OrderDetailsDto>.Failure(
                    Error.Conflict("orders.product-inactive", $"Product '{product.Name}' is not currently active"));
            }

            if (product.StockQuantity < requestedItem.Quantity)
            {
                return Result<OrderDetailsDto>.Failure(OrderErrors.InsufficientStock);
            }

            var addItemResult = order.AddItem(product.Id, product.Name, product.UnitPrice, requestedItem.Quantity);
            if (addItemResult.IsFailure)
            {
                return Result<OrderDetailsDto>.Failure(addItemResult.Error);
            }

            eventItems.Add(new OrderPlacedItemDto(product.Id, requestedItem.Quantity, product.UnitPrice));
        }

        var placeResult = order.Place();
        if (placeResult.IsFailure)
        {
            return Result<OrderDetailsDto>.Failure(placeResult.Error);
        }

        orderRepository.Add(order);

        outboxWriter.Add(new OrderPlacedIntegrationEvent(
            order.Id.Value,
            order.OrderNumber,
            order.TotalAmount,
            eventItems));

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        return saveResult.IsSuccess
            ? Result<OrderDetailsDto>.Success(order.ToDetailsDto(now))
            : Result<OrderDetailsDto>.Failure(saveResult.Error);
    }

    public async Task<Result> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.FindAsync(OrderId.From(orderId), cancellationToken);
        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        var cancelResult = order.Cancel();
        if (cancelResult.IsFailure)
        {
            return cancelResult;
        }

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
