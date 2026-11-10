namespace Zad.Modules.Catalog.Domain.Products;

public interface IProductRepository
{
    public Task<Product?> FindAsync(ProductId id, CancellationToken cancellationToken = default);

    public Task<Product?> FindBySkuAsync(string sku, CancellationToken cancellationToken = default);

    public void Add(Product product);

    public void Remove(Product product);
}
