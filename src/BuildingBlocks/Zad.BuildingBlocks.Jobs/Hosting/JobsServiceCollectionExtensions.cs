using Zad.BuildingBlocks.Persistence.Platform;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Jobs.Schema;
using Zad.BuildingBlocks.Jobs.Scheduling;
using Zad.BuildingBlocks.Jobs.Tenancy;
using Zad.BuildingBlocks.Persistence.Migrations;

namespace Zad.BuildingBlocks.Jobs.Hosting;

public static class JobsServiceCollectionExtensions
{
    public static IServiceCollection AddZadJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JobsOptions>().BindConfiguration(JobsOptions.SectionName).ValidateOnStart();

        var options = configuration.GetSection(JobsOptions.SectionName).Get<JobsOptions>() ?? new JobsOptions();
        var connectionString = configuration.GetConnectionString(PlatformConnectionStrings.Jobs)
            ?? throw new InvalidOperationException("ConnectionStrings:Jobs is required");

        services.AddSingleton<TenantJobFilter>();
        services.AddScoped<ITenantJobScheduler, TenantJobScheduler>();
        services.AddSingleton<IHangfireSchemaProbe, HangfireSchemaProbe>();
        services.AddHostedService<RecurringJobRegistrar>();

        services.AddHangfire((provider, configurationBuilder) => configurationBuilder
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, HangfireStorageConfiguration.StorageOptions())
            .UseActivator(new TenantJobActivator(provider.GetRequiredService<IServiceScopeFactory>()))
            .UseFilter(provider.GetRequiredService<TenantJobFilter>())
            .UseFilter(new AutomaticRetryAttribute { Attempts = options.DefaultRetryAttempts }));

        if (options.ServerEnabled)
        {
            services.AddHangfireServer(serverOptions =>
            {
                serverOptions.ServerName = $"{Environment.MachineName}:{Environment.ProcessId}";
                serverOptions.WorkerCount = options.WorkerCount;
                serverOptions.Queues = options.Queues;
                serverOptions.ShutdownTimeout = TimeSpan.FromSeconds(30);
            });
        }

        return services;
    }
}
