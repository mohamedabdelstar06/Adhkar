using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zad.BuildingBlocks.Contracts.Paging;
using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Catalog.Application;
using Zad.Modules.Catalog.Application.Products;
using Zad.Modules.Catalog.Contracts;
using Zad.Modules.Catalog.Contracts.Events;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Catalog.Domain.Products;
using Zad.Modules.Catalog.Infrastructure.Persistence;
using Zad.TestSupport.Categories;
using Zad.TestSupport.Collections;
using Zad.TestSupport.Databases;
using Zad.TestSupport.Host;

namespace Zad.Modules.Catalog.IntegrationTests.Products;

[Collection(IntegrationTestCollection.Name)]
public sealed class ProductReadRepositoryTests(SqlServerFixture sqlServer) : IntegrationTestBase(sqlServer)
{
    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task ReadRepository_And_UniqueSku_Are_Isolated_Per_Tenant()
    {
        var ct = TestContext.Current.CancellationToken;

        var alphaProduct = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var service = sp.GetRequiredService<ProductLifecycleService>();
            var created = await service.CreateAsync(
                new CreateProductRequest("SHARED-SKU", "Alpha Prayer Rug", "Wool", 120m, 15),
                ct);
            created.IsSuccess.Should().BeTrue();
            return created.Value;
        });

        var bravoProduct = await WithTenantScopeAsync(Tenants.B, async sp =>
        {
            var service = sp.GetRequiredService<ProductLifecycleService>();
            var created = await service.CreateAsync(
                new CreateProductRequest("SHARED-SKU", "Bravo Miswak", "Natural", 15m, 30),
                ct);
            created.IsSuccess.Should().BeTrue();
            return created.Value;
        });

        await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var repo = sp.GetRequiredService<IProductReadRepository>();

            var own = await repo.GetByIdAsync(alphaProduct.Id, ct);
            own.Should().NotBeNull();
            own!.Name.Should().Be("Alpha Prayer Rug");

            var foreign = await repo.GetByIdAsync(bravoProduct.Id, ct);
            foreign.Should().BeNull();

            var batch = await repo.GetByIdsAsync([alphaProduct.Id, bravoProduct.Id], ct);
            batch.Should().ContainSingle(p => p.Id == alphaProduct.Id);

            var list = await repo.ListAsync(1, 20, ct);
            list.Items.Should().ContainSingle(p => p.Id == alphaProduct.Id);
        });
    }

    [Fact]
    [Trait("Category", TestCategories.Rollback)]
    public async Task ExecuteInTransactionAsync_Rolls_Back_On_Failure()
    {
        var ct = TestContext.Current.CancellationToken;

        var txResult = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var repo = sp.GetRequiredService<IProductRepository>();
            var uow = sp.GetRequiredService<ICatalogUnitOfWork>();
            var outbox = sp.GetRequiredService<ICatalogOutboxWriter>();

            return await uow.ExecuteInTransactionAsync(
                async token =>
                {
                    var product = Product.Create("SKU-RBK", "Transient", "Rollback check", 10m, 5).Value;
                    repo.Add(product);
                    outbox.Add(new ProductCreatedIntegrationEvent(product.Id.Value, product.Sku, product.Name, product.UnitPrice));

                    var save = await uow.SaveChangesAsync(token);
                    save.IsSuccess.Should().BeTrue();

                    return Result.Failure(Error.Failure("test.force-rollback", "Deliberate rollback"));
                },
                ct);
        });

        txResult.IsFailure.Should().BeTrue();

        await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var reader = sp.GetRequiredService<IProductReadRepository>();
            var db = sp.GetRequiredService<CatalogDbContext>();

            var listed = await reader.ListAsync(1, 10, ct);
            listed.Items.Should().BeEmpty();
            (await db.Products.CountAsync(ct)).Should().Be(0);
        });
    }

    [Fact]
    [Trait("Category", TestCategories.Concurrency)]
    public async Task Updating_Same_Product_From_Stale_Scope_Returns_Concurrency_Conflict()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await WithTenantScopeAsync(Tenants.A, async sp =>
        {
            var service = sp.GetRequiredService<ProductLifecycleService>();
            var created = await service.CreateAsync(
                new CreateProductRequest("SKU-CONC", "Concurrent Item", "Desc", 50m, 10),
                ct);
            return created.Value.Id;
        });

        using var scope1 = Factory.Services.CreateAsyncScope();
        using var scope2 = Factory.Services.CreateAsyncScope();

        scope1.ServiceProvider.GetRequiredService<Zad.BuildingBlocks.Application.Tenancy.ITenantContextSetter>().Set(Tenants.A.Id);
        scope2.ServiceProvider.GetRequiredService<Zad.BuildingBlocks.Application.Tenancy.ITenantContextSetter>().Set(Tenants.A.Id);

        var repo1 = scope1.ServiceProvider.GetRequiredService<IProductRepository>();
        var uow1 = scope1.ServiceProvider.GetRequiredService<ICatalogUnitOfWork>();
        var p1 = await repo1.FindAsync(ProductId.From(productId), ct);

        var repo2 = scope2.ServiceProvider.GetRequiredService<IProductRepository>();
        var uow2 = scope2.ServiceProvider.GetRequiredService<ICatalogUnitOfWork>();
        var p2 = await repo2.FindAsync(ProductId.From(productId), ct);

        p1!.UpdateDetails("Updated By Scope 1", "Desc", 55m);
        (await uow1.SaveChangesAsync(ct)).IsSuccess.Should().BeTrue();

        p2!.UpdateDetails("Updated By Scope 2", "Desc", 60m);
        var conflict = await uow2.SaveChangesAsync(ct);

        conflict.IsFailure.Should().BeTrue();
        conflict.Error.Type.Should().Be(ErrorType.Conflict);
        conflict.Error.Code.Should().Be(CatalogErrorCodes.ProductNotFound);
    }

    [Fact]
    [Trait("Category", TestCategories.TenantIsolation)]
    public async Task Http_Endpoints_Isolate_Products_Between_Tenant_Hosts()
    {
        var ct = TestContext.Current.CancellationToken;
        using var alphaClient = Factory.CreateClientForTenant(Tenants.A);
        using var bravoClient = Factory.CreateClientForTenant(Tenants.B);

        var createResponse = await alphaClient.PostAsJsonAsync(
            "/api/catalog/products",
            new CreateProductRequest("HTTP-001", "Alpha Lantern", "Brass", 75m, 8),
            ct);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(ct);
        created.Should().NotBeNull();

        var alphaGet = await alphaClient.GetAsync($"/api/catalog/products/{created!.Id}", ct);
        alphaGet.StatusCode.Should().Be(HttpStatusCode.OK);

        var bravoGet = await bravoClient.GetAsync($"/api/catalog/products/{created.Id}", ct);
        bravoGet.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var bravoList = await bravoClient.GetFromJsonAsync<PagedList<ProductListItemDto>>("/api/catalog/products", ct);
        bravoList!.Items.Should().BeEmpty();

        var updateResponse = await alphaClient.PutAsJsonAsync(
            $"/api/catalog/products/{created.Id}",
            new UpdateProductRequest("Alpha Lantern Pro", "Aged Brass", 85m),
            ct);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deactivateResponse = await alphaClient.PostAsync($"/api/catalog/products/{created.Id}/deactivate", null, ct);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updated = await alphaClient.GetFromJsonAsync<ProductDto>($"/api/catalog/products/{created.Id}", ct);
        updated!.Name.Should().Be("Alpha Lantern Pro");
        updated.IsActive.Should().BeFalse();
    }
}

