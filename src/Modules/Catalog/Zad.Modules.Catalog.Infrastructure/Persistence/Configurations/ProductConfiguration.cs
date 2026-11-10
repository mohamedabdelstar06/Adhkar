using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Persistence.Configuration;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : TenantScopedEntityConfiguration<Product, ProductId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", CatalogDbContext.SchemaName);

        builder.Property(product => product.Id)
            .HasConversion(id => id.Value, value => ProductId.From(value));

        builder.Property(product => product.Sku).HasMaxLength(50).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(2000).IsRequired();
        builder.Property(product => product.UnitPrice).HasPrecision(18, 4);

        builder.HasIndex(product => new { product.TenantId, product.Sku })
            .IsUnique()
            .HasDatabaseName("UX_Products_TenantId_Sku");
    }
}
