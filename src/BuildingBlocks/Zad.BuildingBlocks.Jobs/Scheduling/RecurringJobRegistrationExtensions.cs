using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Jobs.Sweeps;

namespace Zad.BuildingBlocks.Jobs.Scheduling;

public static class RecurringJobRegistrationExtensions
{
    public static IServiceCollection AddRecurringJob<TJob>(this IServiceCollection services, string id, string cron)
        where TJob : class
    {
        if (typeof(ITenantJob).IsAssignableFrom(typeof(TJob)))
        {
            throw new InvalidOperationException(
                $"'{typeof(TJob).Name}' is a tenant job. Register it with AddTenantSweep so each tenant gets its own run");
        }

        services.AddScoped<TJob>();
        services.AddSingleton(new RecurringJobDefinition(id, typeof(TJob), cron));

        return services;
    }

    public static IServiceCollection AddTenantSweep<TJob>(this IServiceCollection services, string id, string cron)
        where TJob : class, ITenantJob
    {
        services.AddScoped<TJob>();
        services.AddScoped<TenantSweep<TJob>>();
        services.AddSingleton(new RecurringJobDefinition(id, typeof(TenantSweep<TJob>), cron));

        return services;
    }
}

