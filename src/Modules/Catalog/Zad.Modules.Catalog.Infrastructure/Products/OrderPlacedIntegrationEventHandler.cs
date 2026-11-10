using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Application.Messaging;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Catalog.Application;
using Zad.Modules.Catalog.Domain.Products;
using Zad.Modules.Orders.Contracts.Events;

namespace Zad.Modules.Catalog.Infrastructure.Products;

public sealed class OrderPlacedIntegrationEventHandler(
    IProductRepository productRepository,
    ICatalogUnitOfWork unitOfWork,
    ILogger<OrderPlacedIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
{
    public async Task<Result> HandleAsync(OrderPlacedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing OrderPlaced for order {OrderId} with {ItemCount} items",
            @event.OrderId,
            @event.Items.Count);

        foreach (var item in @event.Items)
        {
            var product = await productRepository.FindAsync(ProductId.From(item.ProductId), cancellationToken);
            if (product is null)
            {
                continue;
            }

            var adjustResult = product.AdjustStock(-item.Quantity);
            if (adjustResult.IsFailure)
            {
                logger.LogWarning(
                    "Could not adjust stock for product {ProductId}: {Error}",
                    item.ProductId,
                    adjustResult.Error.Message);
            }
        }

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
