using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Persistence.Contexts;
using Zad.BuildingBlocks.Persistence.Interceptors;
using Zad.BuildingBlocks.Persistence.Reads;

namespace Zad.BuildingBlocks.Persistence.Registration;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddModuleDbContext<TContext>(
        this IServiceCollection services,
        Func<IServiceProvider, string> connectionStringFactory)
        where TContext : ModuleDbContext
    {
        var descriptor = ModuleDbContextDescriptor.For(typeof(TContext));

        services.AddSingleton(descriptor);
        services.AddScoped<TenantStampingInterceptor>();
        services.AddScoped<AuditStampingInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        services.AddDbContext<TContext>((sp, options) =>
        {
            Sql.ModuleSqlServer.Configure(options, connectionStringFactory(sp), descriptor.Schema);

            options.AddInterceptors(
                sp.GetRequiredService<TenantStampingInterceptor>(),
                sp.GetRequiredService<AuditStampingInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        services.AddScoped<IReadOnlyDbQuery<TContext>, ReadOnlyDbQuery<TContext>>();
        services.AddScoped<IModuleDbContextFactory<TContext>, EngineDbContextFactory<TContext>>();

        return services;
    }

    public static IServiceCollection AddModuleDbContext<TContext>(
        this IServiceCollection services,
        string connectionString)
        where TContext : ModuleDbContext =>
        services.AddModuleDbContext<TContext>(_ => connectionString);
}
