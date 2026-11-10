using AwesomeAssertions;
using Xunit;
using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.Modules.Catalog.Application;
using Zad.Modules.Catalog.Application.Products;
using Zad.Modules.Catalog.Contracts.Events;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.UnitTests.Products;

public sealed class ProductLifecycleServiceTests
{
    private readonly InMemoryProductRepository _repository = new();
    private readonly InMemoryCatalogUnitOfWork _unitOfWork = new();
    private readonly RecordingOutboxWriter _outbox = new();
    private readonly ProductLifecycleService _service;

    public ProductLifecycleServiceTests()
    {
        _service = new ProductLifecycleService(_repository, _unitOfWork, _outbox);
    }

    [Fact]
    public async Task CreateAsync_Rejects_Duplicate_Sku()
    {
        var existing = Product.Create("SKU-DUP", "Item", "Desc", 10m, 2).Value;
        _repository.Add(existing);

        var result = await _service.CreateAsync(
            new CreateProductRequest("SKU-DUP", "Other", "Desc", 20m, 1),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.DuplicateSku);
        _unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_Persists_Product_And_Queues_Created_Event()
    {
        var result = await _service.CreateAsync(
            new CreateProductRequest("SKU-NEW", "Quran Stand", "Wooden", 95m, 12),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("SKU-NEW");
        result.Value.IsActive.Should().BeTrue();
        _unitOfWork.SaveCount.Should().Be(1);
        _outbox.Events.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreatedIntegrationEvent>();
    }

    [Fact]
    public async Task UpdateAsync_Emits_PriceChanged_Event_Only_When_Price_Changes()
    {
        var product = Product.Create("SKU-UPD", "Mat", "Prayer mat", 50m, 10).Value;
        _repository.Add(product);

        var samePrice = await _service.UpdateAsync(
            product.Id.Value,
            new UpdateProductRequest("Deluxe Mat", "Soft", 50m),
            TestContext.Current.CancellationToken);

        samePrice.IsSuccess.Should().BeTrue();
        _outbox.Events.Should().BeEmpty();

        var newPrice = await _service.UpdateAsync(
            product.Id.Value,
            new UpdateProductRequest("Deluxe Mat", "Soft", 65m),
            TestContext.Current.CancellationToken);

        newPrice.IsSuccess.Should().BeTrue();
        var priceEvent = _outbox.Events.Should().ContainSingle()
            .Which.Should().BeOfType<ProductPriceChangedIntegrationEvent>().Subject;
        priceEvent.OldPrice.Should().Be(50m);
        priceEvent.NewPrice.Should().Be(65m);
    }

    [Fact]
    public async Task Operations_Return_NotFound_For_Missing_Product()
    {
        var missingId = GuidV7.New();
        var ct = TestContext.Current.CancellationToken;

        (await _service.UpdateAsync(missingId, new UpdateProductRequest("A", "B", 1m), ct))
            .Error.Should().Be(ProductErrors.NotFound);
        (await _service.AdjustStockAsync(missingId, 1, ct))
            .Error.Should().Be(ProductErrors.NotFound);
        (await _service.DeactivateAsync(missingId, ct))
            .Error.Should().Be(ProductErrors.NotFound);
    }

    private sealed class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _items = [];

        public Task<Product?> FindAsync(ProductId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(p => p.Id == id));

        public Task<Product?> FindBySkuAsync(string sku, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(p => string.Equals(p.Sku, sku, StringComparison.Ordinal)));

        public void Add(Product product) => _items.Add(product);

        public void Remove(Product product) => _items.Remove(product);
    }

    private sealed class InMemoryCatalogUnitOfWork : ICatalogUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> work, CancellationToken cancellationToken = default) =>
            work(cancellationToken);

        public Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> work, CancellationToken cancellationToken = default) =>
            work(cancellationToken);
    }

    private sealed class RecordingOutboxWriter : ICatalogOutboxWriter
    {
        public List<IIntegrationEvent> Events { get; } = [];

        public void Add<TEvent>(TEvent @event) where TEvent : class, IIntegrationEvent =>
            Events.Add(@event);
    }
}

