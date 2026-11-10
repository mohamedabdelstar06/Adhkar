using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Composition;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Messaging.Registration;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Registration;
using Zad.Modules.Catalog.Application.Products;
using Zad.Modules.Catalog.Application;
using Zad.Modules.Catalog.Contracts.Products;
using Zad.Modules.Catalog.Domain.Products;
using Zad.Modules.Catalog.Infrastructure.Persistence;
using Zad.Modules.Catalog.Infrastructure.Persistence.ReadRepositories;
using Zad.Modules.Catalog.Infrastructure.Persistence.Repositories;
using Zad.Modules.Catalog.Infrastructure.Products;
using Zad.Modules.Orders.Contracts.Events;

namespace Zad.Modules.Catalog.Infrastructure;

public sealed class CatalogModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<CatalogDbContext>(provider =>
            provider.GetRequiredService<ITenantConnectionResolver>()
                .ResolveApplicationConnectionString(provider.GetRequiredService<ITenantContext>().TenantId));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<ICatalogUnitOfWork, CatalogUnitOfWork>();
        services.AddScoped<ICatalogOutboxWriter, CatalogOutboxWriter>();

        services.AddScoped<ProductLifecycleService>();
        services.AddScoped<ProductReader>();
        services.AddScoped<IProductReader>(provider => provider.GetRequiredService<ProductReader>());

        services.AddModuleMessaging<CatalogDbContext, ICatalogUnitOfWork>("Catalog", CatalogDbContext.SchemaName);
        services.AddIntegrationEventHandler<OrderPlacedIntegrationEvent, OrderPlacedIntegrationEventHandler>();

        services.AddValidatorsFromAssembly(typeof(CreateProductRequest).Assembly, includeInternalTypes: true);
    }
}
