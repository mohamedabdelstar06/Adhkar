using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.Contexts;
using Zad.MigrationRunner.Cli;
using Zad.MigrationRunner.Discovery;
using Zad.MigrationRunner.Journal;
using Zad.MigrationRunner.Targets;

namespace Zad.MigrationRunner.Execution;

public sealed class MigrationExecutor(
    MigrationTargetSource targetSource,
    ModuleContextDiscovery discovery,
    MigrationJournal journal,
    DatabaseBootstrap bootstrap,
    HangfireSchemaInstaller hangfireInstaller,
    ScriptWriter scriptWriter,
    RunnerLog log)
{
    public async Task<int> RunAsync(RunnerOptions options, CancellationToken cancellationToken)
    {
        var contexts = discovery.Discover().ToLookup(context => context.ModuleName, StringComparer.Ordinal);
        var modules = SelectedModules(options);

        if (options.DryRun)
        {
            scriptWriter.PrintInitialScripts(modules, contexts);
            return 0;
        }

        var anyFailed = false;

        var targets = await targetSource.GetAsync(options, cancellationToken);

        foreach (var target in targets)
        {
            log.Info($"Target {target.DatabaseName} ({target.Kind})");

            await bootstrap.EnsureExistsAsync(target, options.Target, cancellationToken);
            await bootstrap.EnsureReadCommittedSnapshotAsync(target, cancellationToken);
            await journal.EnsureCreatedAsync(target, cancellationToken);

            if (target.Kind == MigrationTargetKind.Jobs)
            {
                await hangfireInstaller.InstallAsync(target, cancellationToken);
                continue;
            }

            if (!await ApplyDatabaseAsync(target, modules, contexts, cancellationToken))
            {
                anyFailed = true;
            }
        }

        return anyFailed ? 1 : 0;
    }

    private static IReadOnlyList<string> SelectedModules(RunnerOptions options)
    {
        if (options.Modules is null)
        {
            return ModuleMigrationOrder.Modules;
        }

        var unknown = options.Modules
            .FirstOrDefault(module => !ModuleMigrationOrder.Modules.Contains(module, StringComparer.OrdinalIgnoreCase));

        if (unknown is not null)
        {
            throw new UnknownModuleException(unknown);
        }

        return ModuleMigrationOrder.Modules
            .Where(module => options.Modules.Contains(module, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task<bool> ApplyDatabaseAsync(
        MigrationTarget target,
        IReadOnlyList<string> modules,
        ILookup<string, DiscoveredModuleContext> contexts,
        CancellationToken cancellationToken)
    {
        var expectedKind = target.Kind == MigrationTargetKind.Catalog
            ? ModuleDatabaseKind.Catalog
            : ModuleDatabaseKind.Tenant;

        foreach (var module in modules)
        {
            foreach (var context in contexts[module].Where(context => context.Kind == expectedKind))
            {
                if (!await ApplyAsync(target, context, cancellationToken))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private async Task<bool> ApplyAsync(
        MigrationTarget target,
        DiscoveredModuleContext context,
        CancellationToken cancellationToken)
    {
        await using var dbContext = ModuleDbContextFactory.Create(context.ContextType, target.ConnectionString);

        var pending = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            log.Info($"{context.ModuleName} is up to date in {target.DatabaseName}");
            return true;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            await journal.RecordAppliedAsync(target, context.ModuleName, pending, stopwatch.ElapsedMilliseconds, cancellationToken);
            log.Info($"{context.ModuleName} applied {pending.Count} migration(s) in {target.DatabaseName}");
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await journal.RecordFailedAsync(target, context.ModuleName, pending, ex.Message, cancellationToken);
            log.Error($"{context.ModuleName} failed in {target.DatabaseName}: {ex.Message}");
            return false;
        }
    }
}
