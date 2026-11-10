using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.MigrationRunner.Execution;

public sealed class ScriptWriter(RunnerLog log)
{
    private const string PlaceholderConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=ZadScriptPreview;Trusted_Connection=True;TrustServerCertificate=True;";

    public void PrintInitialScripts(
        IReadOnlyList<string> modules,
        ILookup<string, Discovery.DiscoveredModuleContext> contexts)
    {
        foreach (var module in modules)
        {
            foreach (var context in contexts[module])
            {
                var script = BuildScript(context, fromMigration: null);
                if (script is null)
                {
                    continue;
                }

                log.Script($"-- {module} / {context.Schema}");
                log.Script(script);
            }
        }
    }

    public string? BuildScript(Discovery.DiscoveredModuleContext context, string? fromMigration)
    {
        using var dbContext = ModuleDbContextFactory.Create(context.ContextType, PlaceholderConnectionString);

        var migrations = dbContext.Database.GetMigrations().ToList();
        if (migrations.Count == 0)
        {
            return null;
        }

        var migrator = dbContext.GetService<IMigrator>();

        return migrator.GenerateScript(
            fromMigration ?? Migration.InitialDatabase,
            migrations[^1],
            MigrationsSqlGenerationOptions.Idempotent);
    }
}
