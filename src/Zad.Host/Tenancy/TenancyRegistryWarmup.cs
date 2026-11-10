using Zad.Modules.Tenancy.Infrastructure.Databases;

namespace Zad.Host.Tenancy;

public sealed class TenancyRegistryWarmup(
    TenancyRegistry registry,
    ILogger<TenancyRegistryWarmup> logger) : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(RefreshInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await registry.RefreshAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Refreshing the tenant database registry failed");
            }
        }
    }
}
