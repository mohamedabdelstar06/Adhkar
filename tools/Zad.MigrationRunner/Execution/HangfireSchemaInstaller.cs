using Hangfire.SqlServer;
using Microsoft.Data.SqlClient;
using Zad.MigrationRunner.Journal;
using Zad.MigrationRunner.Targets;

namespace Zad.MigrationRunner.Execution;

public sealed class HangfireSchemaInstaller(MigrationJournal journal, RunnerLog log)
{
    public async Task InstallAsync(MigrationTarget target, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(target.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        SqlServerObjectsInstaller.Install(connection, schema: "hangfire", enableHeavyMigrations: true);

        string version;

        try
        {
            await using var versionCommand = new SqlCommand("SELECT MAX([Version]) FROM [hangfire].[Schema]", connection);
            version = Convert.ToString(await versionCommand.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture)
                ?? "unknown";
        }
        catch (SqlException ex)
        {
            version = "unknown";
            log.Warn($"Could not read the Hangfire schema version: {ex.Message}");
        }

        await journal.RecordAppliedAsync(target, "Jobs", [$"hangfire-schema-{version}"], 0, cancellationToken);
        log.Info($"Hangfire schema installed in {target.DatabaseName} (version {version})");
    }
}
