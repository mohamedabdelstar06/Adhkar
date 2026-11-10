using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Persistence.UnitOfWork;
using Zad.Modules.Catalog.Application;
using Zad.Modules.Catalog.Contracts;
using Zad.Modules.Catalog.Domain.Products;

namespace Zad.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogUnitOfWork(CatalogDbContext context)
    : ModuleUnitOfWork<CatalogDbContext>(context), ICatalogUnitOfWork
{
    protected override Error ConcurrencyConflict =>
        Error.Conflict(CatalogErrorCodes.ProductNotFound, "The product was modified concurrently by another process");

    protected override IReadOnlyDictionary<string, Error> UniqueIndexErrors { get; } =
        new Dictionary<string, Error>(StringComparer.OrdinalIgnoreCase)
        {
            ["UX_Products_TenantId_Sku"] = ProductErrors.DuplicateSku
        };
}
