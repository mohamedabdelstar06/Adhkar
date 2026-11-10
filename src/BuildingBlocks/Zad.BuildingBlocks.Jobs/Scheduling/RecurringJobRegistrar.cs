using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zad.BuildingBlocks.Jobs.Scheduling;

public sealed class RecurringJobRegistrar(
    IRecurringJobManager recurringJobs,
    JobStorage storage,
    IEnumerable<RecurringJobDefinition> definitions,
    ILogger<RecurringJobRegistrar> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var registered = definitions.ToList();

        foreach (var definition in registered)
        {
            var job = BuildJob(definition.JobType);

            recurringJobs.AddOrUpdate(
                definition.Id,
                job,
                definition.Cron,
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

            logger.LogInformation("Recurring job {JobId} registered with cron {Cron}", definition.Id, definition.Cron);
        }

        RemoveStale(registered);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Job BuildJob(Type jobType)
    {
        var runAsync = jobType.GetMethod("RunAsync", [typeof(CancellationToken)])
            ?? throw new InvalidOperationException($"Job '{jobType.Name}' must expose Task RunAsync(CancellationToken)");

        return new Job(jobType, runAsync, CancellationToken.None);
    }

    private void RemoveStale(IReadOnlyList<RecurringJobDefinition> registered)
    {
        var known = registered.Select(definition => definition.Id).ToHashSet(StringComparer.Ordinal);
        var existing = storage.GetConnection().GetRecurringJobs();

        foreach (var stale in existing.Where(job => !known.Contains(job.Id)))
        {
            recurringJobs.RemoveIfExists(stale.Id);
            logger.LogInformation("Recurring job {JobId} removed because it is no longer registered", stale.Id);
        }
    }
}
