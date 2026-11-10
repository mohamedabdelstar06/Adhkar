using Microsoft.Extensions.Configuration;
using Zad.MigrationRunner.Cli;
using Zad.MigrationRunner.Discovery;
using Zad.MigrationRunner.Execution;
using Zad.MigrationRunner.Journal;
using Zad.MigrationRunner.Seeding;
using Zad.MigrationRunner.Targets;
using Zad.MigrationRunner.Tenancy;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Local.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var log = new RunnerLog();

try
{
    var options = RunnerOptions.Parse(args);

    if (options.AddTenant is not null)
    {
        var onboarded = await new TenantOnboarding(configuration, log).RegisterAsync(
            options.AddTenant,
            options.Host!,
            options.DefaultLanguage,
            options.TimeZone,
            CancellationToken.None);

        return onboarded ? 0 : 1;
    }

    var executor = new MigrationExecutor(
        new MigrationTargetSource(configuration),
        new ModuleContextDiscovery(),
        new MigrationJournal(),
        new DatabaseBootstrap(log),
        new HangfireSchemaInstaller(new MigrationJournal(), log),
        new ScriptWriter(log),
        log);

    var exitCode = await executor.RunAsync(options, CancellationToken.None);

    if (exitCode == 0 && options.SeedDev)
    {
        var seeded = await new DevEnvironmentSeeder(configuration, log).SeedAsync(CancellationToken.None);
        return seeded ? 0 : 1;
    }

    return exitCode;
}
catch (ArgumentException ex)
{
    log.Error(ex.Message);
    return 2;
}
catch (UnknownModuleException ex)
{
    log.Error(ex.Message);
    return 2;
}
catch (Exception ex)
{
    log.Error(ex.ToString());
    return 1;
}
