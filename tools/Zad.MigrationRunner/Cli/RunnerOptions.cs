namespace Zad.MigrationRunner.Cli;

public sealed record RunnerOptions(
    RunnerTarget Target,
    DatabaseSelector Databases,
    string? TenantSlug,
    IReadOnlyList<string>? Modules,
    bool DryRun,
    string? ScriptPath,
    bool SeedDev,
    string? AddTenant,
    string? Host,
    string DefaultLanguage,
    string TimeZone)
{
    public static RunnerOptions Parse(string[] args)
    {
        RunnerTarget? target = null;
        var databases = DatabaseSelector.All;
        var databasesGiven = false;
        string? tenantSlug = null;
        IReadOnlyList<string>? modules = null;
        var dryRun = false;
        string? scriptPath = null;
        var seedDev = false;
        string? addTenant = null;
        string? host = null;
        var defaultLanguage = "ar";
        var timeZone = "UTC";

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];

            switch (argument)
            {
                case "--target":
                    target = ParseTarget(Next(args, ref index, argument));
                    break;
                case "--databases":
                    databases = ParseDatabases(Next(args, ref index, argument));
                    databasesGiven = true;
                    break;
                case "--tenant":
                    tenantSlug = Next(args, ref index, argument);
                    break;
                case "--module":
                    modules = Next(args, ref index, argument)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                case "--script":
                    scriptPath = Next(args, ref index, argument);
                    break;
                case "--seed-dev":
                    seedDev = true;
                    break;
                case "--add-tenant":
                    addTenant = Next(args, ref index, argument);
                    break;
                case "--host":
                    host = Next(args, ref index, argument);
                    break;
                case "--language":
                    defaultLanguage = Next(args, ref index, argument);
                    break;
                case "--timezone":
                    timeZone = Next(args, ref index, argument);
                    break;
                default:
                    throw new ArgumentException($"Unknown option '{argument}'.\n{RunnerUsage.Text}");
            }
        }

        if (addTenant is not null)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException($"--add-tenant requires --host <host>.\n{RunnerUsage.Text}");
            }

            if (dryRun || scriptPath is not null)
            {
                throw new ArgumentException($"--add-tenant does not migrate; drop --dry-run and --script.\n{RunnerUsage.Text}");
            }

            return new RunnerOptions(
                target ?? RunnerTarget.Local,
                databases,
                tenantSlug,
                modules,
                dryRun,
                scriptPath,
                seedDev,
                addTenant,
                host,
                defaultLanguage,
                timeZone);
        }

        if (target is null)
        {
            throw new ArgumentException($"--target is required.\n{RunnerUsage.Text}");
        }

        if (dryRun && scriptPath is not null)
        {
            throw new ArgumentException($"--dry-run and --script are mutually exclusive.\n{RunnerUsage.Text}");
        }

        if (tenantSlug is not null && databasesGiven)
        {
            throw new ArgumentException($"--tenant selects its own database; drop --databases.\n{RunnerUsage.Text}");
        }

        if (seedDev && target != RunnerTarget.Local)
        {
            throw new ArgumentException($"--seed-dev is local only.\n{RunnerUsage.Text}");
        }

        return new RunnerOptions(
            target.Value,
            databases,
            tenantSlug,
            modules,
            dryRun,
            scriptPath,
            seedDev,
            addTenant,
            host,
            defaultLanguage,
            timeZone);
    }

    public bool Includes(DatabaseSelector selector) =>
        Databases == DatabaseSelector.All || Databases == selector;

    private static string Next(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"{option} requires a value.\n{RunnerUsage.Text}");
        }

        index++;
        return args[index];
    }

    private static RunnerTarget ParseTarget(string value) => value.ToLowerInvariant() switch
    {
        "local" => RunnerTarget.Local,
        "ci" => RunnerTarget.Ci,
        "staging" => RunnerTarget.Staging,
        "production" => RunnerTarget.Production,
        _ => throw new ArgumentException($"Unknown target '{value}'.\n{RunnerUsage.Text}")
    };

    private static DatabaseSelector ParseDatabases(string value) => value.ToLowerInvariant() switch
    {
        "catalog" => DatabaseSelector.Catalog,
        "shared" => DatabaseSelector.Shared,
        "dedicated" => DatabaseSelector.Dedicated,
        "jobs" => DatabaseSelector.Jobs,
        "all" => DatabaseSelector.All,
        _ => throw new ArgumentException($"Unknown database selector '{value}'.\n{RunnerUsage.Text}")
    };
}
