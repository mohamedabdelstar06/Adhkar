using Microsoft.EntityFrameworkCore;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(CatalogDbContext context) : IProductRepository
{
    public Task<Product?> FindAsync(ProductId id, CancellationToken cancellationToken = default) =>
        context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<Product?> FindBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim().ToUpperInvariant();
        return context.Products.FirstOrDefaultAsync(product => product.Sku == normalized, cancellationToken);
    }

    public void Add(Product product) => context.Products.Add(product);

    public void Remove(Product product) => context.Products.Remove(product);
}
