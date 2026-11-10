using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Composition;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Messaging.Registration;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Registration;
using Zad.Modules.Orders.Application.Orders;
using Zad.Modules.Orders.Application;
using Zad.Modules.Orders.Contracts.Orders;
using Zad.Modules.Orders.Domain.Orders;
using Zad.Modules.Orders.Infrastructure.Persistence;
using Zad.Modules.Orders.Infrastructure.Persistence.ReadRepositories;
using Zad.Modules.Orders.Infrastructure.Persistence.Repositories;

namespace Zad.Modules.Orders.Infrastructure;

public sealed class OrdersModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<OrdersDbContext>(provider =>
            provider.GetRequiredService<ITenantConnectionResolver>()
                .ResolveApplicationConnectionString(provider.GetRequiredService<ITenantContext>().TenantId));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrdersUnitOfWork, OrdersUnitOfWork>();
        services.AddScoped<IOrdersOutboxWriter, OrdersOutboxWriter>();
        services.AddModuleMessaging<OrdersDbContext, IOrdersUnitOfWork>("Orders", OrdersDbContext.SchemaName);

        services.AddScoped<OrderPlacementService>();
        services.AddScoped<OrderReader>();
        services.AddScoped<IOrderReader>(provider => provider.GetRequiredService<OrderReader>());

        services.AddValidatorsFromAssembly(typeof(CreateOrderRequest).Assembly, includeInternalTypes: true);
    }
}
