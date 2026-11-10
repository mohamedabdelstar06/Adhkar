using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Catalog.Contracts.Events;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Application.Products;

public sealed class ProductLifecycleService(
    IProductRepository productRepository,
    ICatalogUnitOfWork unitOfWork,
    ICatalogOutboxWriter outboxWriter)
{
    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await productRepository.FindBySkuAsync(request.Sku, cancellationToken);
        if (existing is not null)
        {
            return Result<ProductDto>.Failure(ProductErrors.DuplicateSku);
        }

        var createResult = Product.Create(
            request.Sku,
            request.Name,
            request.Description,
            request.UnitPrice,
            request.StockQuantity);

        if (createResult.IsFailure)
        {
            return Result<ProductDto>.Failure(createResult.Error);
        }

        var product = createResult.Value;
        productRepository.Add(product);

        outboxWriter.Add(new ProductCreatedIntegrationEvent(
            product.Id.Value,
            product.Sku,
            product.Name,
            product.UnitPrice));

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        return saveResult.IsSuccess
            ? Result<ProductDto>.Success(product.ToDto())
            : Result<ProductDto>.Failure(saveResult.Error);
    }

    public async Task<Result> UpdateAsync(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.FindAsync(ProductId.From(productId), cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound);
        }

        var oldPrice = product.UnitPrice;
        var updateResult = product.UpdateDetails(request.Name, request.Description, request.UnitPrice);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        if (oldPrice != request.UnitPrice)
        {
            outboxWriter.Add(new ProductPriceChangedIntegrationEvent(product.Id.Value, oldPrice, request.UnitPrice));
        }

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result> AdjustStockAsync(Guid productId, int delta, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.FindAsync(ProductId.From(productId), cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound);
        }

        var adjustResult = product.AdjustStock(delta);
        if (adjustResult.IsFailure)
        {
            return adjustResult;
        }

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result> DeactivateAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.FindAsync(ProductId.From(productId), cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound);
        }

        var deactivateResult = product.Deactivate();
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
