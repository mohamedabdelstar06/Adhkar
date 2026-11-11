using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Composition;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Platform;
using Zad.BuildingBlocks.Persistence.Registration;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Application.Security;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Application;
using Zad.Modules.Tenancy.Contracts;
using Zad.Modules.Tenancy.Domain.Tenants;
using Zad.Modules.Tenancy.Infrastructure.Databases;
using Zad.Modules.Tenancy.Infrastructure.Persistence;
using Zad.Modules.Tenancy.Infrastructure.Persistence.ReadRepositories;
using Zad.Modules.Tenancy.Infrastructure.Persistence.Repositories;
using Zad.Modules.Tenancy.Infrastructure.Security;

using Zad.Modules.Tenancy.Domain.Databases;

namespace Zad.Modules.Tenancy.Infrastructure;

public sealed class TenancyModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        var catalogConnectionString = configuration.GetConnectionString(PlatformConnectionStrings.Catalog)
            ?? throw new InvalidOperationException("ConnectionStrings:Catalog is required");

        services.AddModuleDbContext<TenancyDbContext>(catalogConnectionString);

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantDatabaseRepository, TenantDatabaseRepository>();
        services.AddScoped<ITenantReadRepository, TenantReadRepository>();
        services.AddScoped<IDatabaseReadRepository, DatabaseReadRepository>();
        services.AddScoped<ITenancyUnitOfWork, TenancyUnitOfWork>();

        services.AddSingleton<IConnectionStringProtector, DataProtectionConnectionStringProtector>();
        services.AddSingleton<TenancyRegistry>();
        services.AddSingleton<IDatabaseRegistry>(provider => provider.GetRequiredService<TenancyRegistry>());
        services.AddSingleton<ITenantConnectionResolver>(provider => provider.GetRequiredService<TenancyRegistry>());
        services.AddSingleton<ITenancyCacheRefresher>(provider => provider.GetRequiredService<TenancyRegistry>());

        services.AddScoped<TenantProvisioningService>();
        services.AddScoped<CurrentTenantReader>();
        services.AddScoped<TenantAdminReader>();
        services.AddScoped<TenantReader>();
        services.AddScoped<ITenantDirectory>(provider => provider.GetRequiredService<TenantReader>());

        services.AddValidatorsFromAssembly(typeof(RegisterTenantRequest).Assembly, includeInternalTypes: true);
    }
}
