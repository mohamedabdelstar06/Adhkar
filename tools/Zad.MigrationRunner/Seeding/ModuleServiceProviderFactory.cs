using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Composition;
using Zad.BuildingBlocks.Application.Identity;
using Zad.BuildingBlocks.Application.Localization;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.DataProtection;
using Zad.BuildingBlocks.Persistence.Tenancy;
using Zad.Modules.Adhkar.Infrastructure;
using Zad.Modules.Tenancy.Infrastructure;

namespace Zad.MigrationRunner.Seeding;

public static class ModuleServiceProviderFactory
{
    public static ServiceProvider Build(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        services.AddSingleton(TimeProvider.System);
        services.AddLogging();
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantContextSetter>(provider => provider.GetRequiredService<TenantContext>());
        services.AddScoped<ICurrentUser, RunnerCurrentUser>();
        services.AddScoped<RunnerCorrelationContext>();
        services.AddScoped<ICorrelationContext>(provider => provider.GetRequiredService<RunnerCorrelationContext>());
        services.AddScoped<ICorrelationContextSetter>(provider => provider.GetRequiredService<RunnerCorrelationContext>());
        services.AddScoped<RunnerCultureContext>();
        services.AddScoped<ICultureContext>(provider => provider.GetRequiredService<RunnerCultureContext>());
        services.AddScoped<ICultureContextSetter>(provider => provider.GetRequiredService<RunnerCultureContext>());

        services.AddZadDataProtection(configuration);
        services.AddModules(
            configuration,
            typeof(TenancyModuleInstaller).Assembly,
            typeof(AdhkarModuleInstaller).Assembly);

        return services.BuildServiceProvider();
    }
}
