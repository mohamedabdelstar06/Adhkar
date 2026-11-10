using Zad.BuildingBlocks.Persistence.Platform;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zad.BuildingBlocks.Application.Migrations;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Persistence.Migrations;

public sealed class PendingMigrationsGuard(
    IEnumerable<ModuleDbContextDescriptor> descriptors,
    IDatabaseRegistry registry,
    ITenantConnectionResolver connections,
    IHangfireSchemaProbe hangfireProbe,
    IConfiguration configuration,
    TimeProvider timeProvider) : ITenantDatabaseReadiness
{
    private static readonly TimeSpan BehindCacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan CurrentCacheDuration = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<Guid, (bool IsCurrent, DateTimeOffset CheckedAt)> _cache = new();

    public async Task<MigrationCurrencyReport> CheckStartupDatabasesAsync(CancellationToken cancellationToken = default)
    {
        var pending = new List<PendingMigration>();
        var all = descriptors.ToList();

        var catalogConnectionString = configuration.GetConnectionString(PlatformConnectionStrings.Catalog);
        if (catalogConnectionString is null)
        {
            pending.Add(new PendingMigration("ZadCatalog", "Platform", "connection-string-missing"));
        }
        else
        {
            foreach (var descriptor in all.Where(d => d.Kind == ModuleDatabaseKind.Catalog))
            {
                pending.AddRange(await PendingAsync(descriptor, catalogConnectionString, "ZadCatalog", cancellationToken));
            }
        }

        foreach (var database in registry.List().Where(d => d.Kind == TenantDatabaseKind.Shared && d.Status == TenantDatabaseStatus.Online))
        {
            var connectionString = await connections.ResolveMigratorConnectionStringAsync(database.Id, cancellationToken);

            foreach (var descriptor in all.Where(d => d.Kind == ModuleDatabaseKind.Tenant))
            {
                pending.AddRange(await PendingAsync(descriptor, connectionString, database.Name, cancellationToken));
            }
        }

        var jobsConnectionString = configuration.GetConnectionString(PlatformConnectionStrings.Jobs);
        if (jobsConnectionString is null || !await hangfireProbe.IsInstalledAsync(jobsConnectionString, cancellationToken))
        {
            pending.Add(new PendingMigration("ZadJobs", "Jobs", "hangfire-schema"));
        }

        return new MigrationCurrencyReport(pending);
    }

    public async Task EnsureStartupDatabasesCurrentAsync(CancellationToken cancellationToken = default)
    {
        var report = await CheckStartupDatabasesAsync(cancellationToken);

        if (report.IsCurrent)
        {
            return;
        }

        throw new InvalidOperationException(
            "The application cannot start because at least one database is behind on migrations:" +
            Environment.NewLine +
            report.Describe() +
            Environment.NewLine +
            "Run: dotnet run --project tools/Zad.MigrationRunner -- --target local");
    }

    public async Task<bool> IsReadyAsync(Guid databaseId, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(databaseId, out var cached))
        {
            var window = cached.IsCurrent ? CurrentCacheDuration : BehindCacheDuration;
            if (timeProvider.GetUtcNow() - cached.CheckedAt < window)
            {
                return cached.IsCurrent;
            }
        }

        var connectionString = await connections.ResolveMigratorConnectionStringAsync(databaseId, cancellationToken);
        var databaseName = registry.Find(databaseId)?.Name ?? databaseId.ToString();

        var pending = new List<PendingMigration>();

        foreach (var descriptor in descriptors.Where(d => d.Kind == ModuleDatabaseKind.Tenant))
        {
            pending.AddRange(await PendingAsync(descriptor, connectionString, databaseName, cancellationToken));
        }

        var isCurrent = pending.Count == 0;
        _cache[databaseId] = (isCurrent, timeProvider.GetUtcNow());

        return isCurrent;
    }

    private static async Task<IReadOnlyList<PendingMigration>> PendingAsync(
        ModuleDbContextDescriptor descriptor,
        string connectionString,
        string databaseName,
        CancellationToken cancellationToken)
    {
        await using var context = ModuleDbContextFactory.Create(descriptor.ContextType, connectionString);
        var pending = await context.Database.GetPendingMigrationsAsync(cancellationToken);

        return pending.Select(id => new PendingMigration(databaseName, descriptor.Module, id)).ToList();
    }
}
