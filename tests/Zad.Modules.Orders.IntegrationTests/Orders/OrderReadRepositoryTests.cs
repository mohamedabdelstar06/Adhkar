using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zad.BuildingBlocks.Contracts.Paging;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Messaging.Inbox;
using Zad.BuildingBlocks.Messaging.Outbox;
using Zad.Modules.Catalog.Application.Products;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Catalog.Infrastructure.Persistence;
using Zad.Modules.Catalog.Infrastructure.Products;
using Zad.Modules.Orders.Application.Orders;
using Zad.Modules.Orders.Contracts.Events;
using Zad.Modules.Orders.Contracts.Orders;
using Zad.Modules.Orders.Domain.Orders;
using Zad.Modules.Orders.Infrastructure.Persistence;
using Zad.TestSupport.Categories;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;
using Zad.TestSupport.Host;

namespace Zad.Modules.Orders.IntegrationTests.Orders;

[Collection(IntegrationTestCollection.Name)]
public sealed class OrderReadRepositoryTests(SqlServerFixture sqlServer) : IntegrationTestBase(sqlServer)
{
    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task ReadRepository_Isolates_Orders_And_Persists_Outbox_Atomically()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var service = sp.GetRequiredService<ProductLifecycleService>();
            var created = await service.CreateAsync(new CreateProductRequest("ORD-SKU-1", "Dates Box", "Madinah", 40m, 10), ct);
            return created.Value.Id;
        });

        var order = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var placement = sp.GetRequiredService<OrderPlacementService>();
            var created = await placement.CreateOrderAsync(
                new CreateOrderRequest("customer@alpha.test", [new CreateOrderItemRequest(productId, 2)]),
                ct);
            created.IsSuccess.Should().BeTrue();
            return created.Value;
        });

        await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var repo = sp.GetRequiredService<IOrderReadRepository>();
            var db = sp.GetRequiredService<OrdersDbContext>();

            var loaded = await repo.GetByIdAsync(order.Id, ct);
            loaded.Should().NotBeNull();
            loaded!.TotalAmount.Should().Be(80m);
            loaded.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2);

            var listed = await repo.ListAsync(1, 20, ct);
            listed.Items.Should().ContainSingle(o => o.Id == order.Id);

            var outboxMessages = await db.Set<OutboxMessage>().ToListAsync(ct);
            outboxMessages.Should().ContainSingle(m => m.TenantId == Tenants.A.Id && m.EventName == "orders.order-placed.v1");
        });

        await WithTenantScopeAsync(Tenants.B, async sp =>
        {
            var repo = sp.GetRequiredService<IOrderReadRepository>();
            (await repo.GetByIdAsync(order.Id, ct)).Should().BeNull();
            (await repo.ListAsync(1, 20, ct)).Items.Should().BeEmpty();
        });
    }

    [Fact]
    [Trait("Category", TestCategories.Rollback)]
    public async Task Insufficient_Stock_Fails_Without_Saving_Order_Or_Outbox()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var service = sp.GetRequiredService<ProductLifecycleService>();
            var created = await service.CreateAsync(new CreateProductRequest("ORD-LOW", "Rare Oud", "Oil", 100m, 1), ct);
            return created.Value.Id;
        });

        var failed = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var placement = sp.GetRequiredService<OrderPlacementService>();
            return await placement.CreateOrderAsync(
                new CreateOrderRequest("buyer@alpha.test", [new CreateOrderItemRequest(productId, 5)]),
                ct);
        });

        failed.IsFailure.Should().BeTrue();
        failed.Error.Should().Be(OrderErrors.InsufficientStock);

        await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var db = sp.GetRequiredService<OrdersDbContext>();
            (await db.Orders.CountAsync(ct)).Should().Be(0);
            (await db.Set<OutboxMessage>().CountAsync(ct)).Should().Be(0);
        });
    }

    [Fact]
    [Trait("Category", TestCategories.DuplicateDelivery)]
    public async Task OrderPlaced_Handler_Deducts_Catalog_Stock_And_Inbox_Rejects_Duplicate_Delivery()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var service = sp.GetRequiredService<ProductLifecycleService>();
            var created = await service.CreateAsync(new CreateProductRequest("ORD-STK", "Miswak", "Pack", 10m, 10), ct);
            return created.Value.Id;
        });

        await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var handler = sp.GetRequiredService<OrderPlacedIntegrationEventHandler>();
            var handled = await handler.HandleAsync(
                new OrderPlacedIntegrationEvent(GuidV7.New(), "ORD-TEST-1", 30m, [new OrderPlacedItemDto(productId, 3, 10m)]),
                ct);
            handled.IsSuccess.Should().BeTrue();

            var productReader = sp.GetRequiredService<IProductReader>();
            var updated = await productReader.GetByIdAsync(productId, ct);
            updated!.StockQuantity.Should().Be(7);

            var catalogDb = sp.GetRequiredService<CatalogDbContext>();
            var time = sp.GetRequiredService<TimeProvider>();
            var messageId = GuidV7.New();
            var handlerName = typeof(OrderPlacedIntegrationEventHandler).FullName!;

            catalogDb.Set<InboxMessage>().Add(InboxMessage.Create(
                GuidV7.New(),
                messageId,
                handlerName,
                Tenants.A.Id,
                "orders.order-placed.v1",
                "{}",
                "corr-1",
                null,
                time.GetUtcNow()));
            await catalogDb.SaveChangesAsync(ct);

            catalogDb.Set<InboxMessage>().Add(InboxMessage.Create(
                GuidV7.New(),
                messageId,
                handlerName,
                Tenants.A.Id,
                "orders.order-placed.v1",
                "{}",
                "corr-1",
                null,
                time.GetUtcNow()));

            var duplicateAct = async () => await catalogDb.SaveChangesAsync(ct);
            await duplicateAct.Should().ThrowAsync<DbUpdateException>();
        });
    }

    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Http_Endpoints_Create_List_And_Cancel_Orders_Within_Tenant()
    {
        var ct = TestContext.Current.CancellationToken;
        using var alphaClient = Factory.CreateClientForTenant(Tenants.A);
        using var bravoClient = Factory.CreateClientForTenant(Tenants.B);

        var productResponse = await alphaClient.PostAsJsonAsync(
            "/api/catalog/products",
            new CreateProductRequest("ORD-HTTP", "Rug", "Cotton", 60m, 12),
            ct);
        var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>(ct);

        var createResponse = await alphaClient.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest("buyer@alpha.test", [new CreateOrderItemRequest(product!.Id, 2)]),
            ct);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await createResponse.Content.ReadFromJsonAsync<OrderDetailsDto>(ct);

        var alphaGet = await alphaClient.GetAsync($"/api/orders/{order!.Id}", ct);
        alphaGet.StatusCode.Should().Be(HttpStatusCode.OK);

        var bravoGet = await bravoClient.GetAsync($"/api/orders/{order.Id}", ct);
        bravoGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var cancelResponse = await alphaClient.PostAsync($"/api/orders/{order.Id}/cancel", null, ct);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var list = await alphaClient.GetFromJsonAsync<PagedList<OrderListItemDto>>("/api/orders", ct);
        list!.Items.Should().ContainSingle(o => o.Id == order.Id && o.Status == "Cancelled");
    }
}
