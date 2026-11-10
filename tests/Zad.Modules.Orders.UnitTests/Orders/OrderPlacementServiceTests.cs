using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Paging;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Orders.Application;
using Zad.Modules.Orders.Application.Orders;
using Zad.Modules.Orders.Contracts.Events;
using Zad.Modules.Orders.Domain.Orders;

namespace Zad.Modules.Orders.UnitTests.Orders;

public sealed class OrderPlacementServiceTests
{
    private readonly InMemoryOrderRepository _repository = new();
    private readonly StubProductReader _productReader = new();
    private readonly InMemoryOrdersUnitOfWork _unitOfWork = new();
    private readonly RecordingOutboxWriter _outbox = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 10, 12, 30, 45, 123, TimeSpan.Zero));
    private readonly OrderPlacementService _service;

    public OrderPlacementServiceTests()
    {
        _service = new OrderPlacementService(_repository, _productReader, _unitOfWork, _outbox, _time);
    }

    [Fact]
    public void Validator_Rejects_Invalid_Email_And_Empty_Items()
    {
        var validator = new CreateOrderRequestValidator();
        var invalid = validator.Validate(new CreateOrderRequest("not-an-email", []));

        invalid.IsValid.Should().BeFalse();
        invalid.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.CustomerEmail));
        invalid.Errors.Should().Contain(e => e.PropertyName == nameof(CreateOrderRequest.Items));
    }

    [Fact]
    public async Task CreateOrderAsync_Returns_ProductNotFound_When_Product_Does_Not_Exist()
    {
        var result = await _service.CreateOrderAsync(
            new CreateOrderRequest("user@zad.test", [new CreateOrderItemRequest(GuidV7.New(), 1)]),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.ProductNotFound);
    }

    [Fact]
    public async Task CreateOrderAsync_Rejects_Inactive_Or_Understocked_Product()
    {
        var inactiveId = GuidV7.New();
        _productReader.Products[inactiveId] = new ProductDto(inactiveId, "SKU-IN", "Inactive", "", 10m, 5, false);

        var inactiveResult = await _service.CreateOrderAsync(
            new CreateOrderRequest("user@zad.test", [new CreateOrderItemRequest(inactiveId, 1)]),
            TestContext.Current.CancellationToken);

        inactiveResult.IsFailure.Should().BeTrue();
        inactiveResult.Error.Code.Should().Be("orders.product-inactive");

        var lowStockId = GuidV7.New();
        _productReader.Products[lowStockId] = new ProductDto(lowStockId, "SKU-LOW", "LowStock", "", 10m, 2, true);

        var stockResult = await _service.CreateOrderAsync(
            new CreateOrderRequest("user@zad.test", [new CreateOrderItemRequest(lowStockId, 3)]),
            TestContext.Current.CancellationToken);

        stockResult.IsFailure.Should().BeTrue();
        stockResult.Error.Should().Be(OrderErrors.InsufficientStock);
    }

    [Fact]
    public async Task CreateOrderAsync_Uses_TimeProvider_Saves_Order_And_Emits_Outbox_Event()
    {
        var productId = GuidV7.New();
        _productReader.Products[productId] = new ProductDto(productId, "SKU-OK", "Tasbih", "Wood", 15m, 10, true);

        var result = await _service.CreateOrderAsync(
            new CreateOrderRequest("buyer@zad.test", [new CreateOrderItemRequest(productId, 2)]),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.OrderNumber.Should().StartWith("ORD-20261110123045-");
        result.Value.TotalAmount.Should().Be(30m);
        result.Value.CreatedAtUtc.Should().Be(_time.GetUtcNow());
        _unitOfWork.SaveCount.Should().Be(1);

        var placed = _outbox.Events.Should().ContainSingle()
            .Which.Should().BeOfType<OrderPlacedIntegrationEvent>().Subject;
        placed.OrderId.Should().Be(result.Value.Id);
        placed.TotalAmount.Should().Be(30m);
        placed.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2);
    }

    [Fact]
    public async Task CancelOrderAsync_Returns_NotFound_Or_Cancels_Existing_Order()
    {
        var ct = TestContext.Current.CancellationToken;
        (await _service.CancelOrderAsync(GuidV7.New(), ct)).Error.Should().Be(OrderErrors.NotFound);

        var productId = GuidV7.New();
        _productReader.Products[productId] = new ProductDto(productId, "SKU-C", "Book", "", 20m, 5, true);
        var created = await _service.CreateOrderAsync(
            new CreateOrderRequest("buyer@zad.test", [new CreateOrderItemRequest(productId, 1)]),
            ct);

        var cancelled = await _service.CancelOrderAsync(created.Value.Id, ct);

        cancelled.IsSuccess.Should().BeTrue();
        _unitOfWork.SaveCount.Should().Be(2);
    }

    private sealed class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = [];

        public Task<Order?> FindAsync(OrderId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

        public Task<Order?> FindByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default) =>
            Task.FromResult(_orders.FirstOrDefault(o => string.Equals(o.OrderNumber, orderNumber, StringComparison.Ordinal)));

        public void Add(Order order) => _orders.Add(order);

        public void Remove(Order order) => _orders.Remove(order);
    }

    private sealed class StubProductReader : IProductReader
    {
        public Dictionary<Guid, ProductDto> Products { get; } = [];

        public Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Products.GetValueOrDefault(productId));

        public Task<IReadOnlyList<ProductDto>> GetByIdsAsync(IReadOnlyList<Guid> productIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ProductDto>>(
                productIds.Where(Products.ContainsKey).Select(id => Products[id]).ToList());

        public Task<PagedList<ProductListItemDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PagedList<ProductListItemDto>([], page, pageSize, 0));
    }

    private sealed class InMemoryOrdersUnitOfWork : IOrdersUnitOfWork
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

    private sealed class RecordingOutboxWriter : IOrdersOutboxWriter
    {
        public List<IIntegrationEvent> Events { get; } = [];

        public void Add<TEvent>(TEvent @event) where TEvent : class, IIntegrationEvent =>
            Events.Add(@event);
    }
}

