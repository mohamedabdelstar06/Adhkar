using Microsoft.Data.SqlClient;
using Zad.MigrationRunner.Cli;
using Zad.MigrationRunner.Targets;

namespace Zad.MigrationRunner.Execution;

public sealed class DatabaseBootstrap(RunnerLog log)
{
    public async Task EnsureExistsAsync(MigrationTarget target, RunnerTarget runnerTarget, CancellationToken cancellationToken)
    {
        if (runnerTarget is not (RunnerTarget.Local or RunnerTarget.Ci))
        {
            return;
        }

        var builder = new SqlConnectionStringBuilder(target.ConnectionString) { InitialCatalog = "master" };

        try
        {
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(
                """
                IF DB_ID(@name) IS NULL
                BEGIN
                    DECLARE @create nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@name);
                    EXEC sp_executesql @create;
                END
                """,
                connection);
            command.Parameters.AddWithValue("@name", target.DatabaseName);

            await command.ExecuteNonQueryAsync(cancellationToken);
            log.Info($"Database {target.DatabaseName} ensured");
        }
        catch (SqlException ex)
        {
            log.Warn($"Could not create {target.DatabaseName} automatically: {ex.Message}");
        }
    }

    public async Task EnsureReadCommittedSnapshotAsync(MigrationTarget target, CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(target.ConnectionString) { InitialCatalog = "master" };

        try
        {
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(
                """
                IF EXISTS (SELECT 1 FROM sys.databases
                           WHERE name = @name AND is_read_committed_snapshot_on = 0)
                BEGIN
                    DECLARE @alter nvarchar(max) =
                        N'ALTER DATABASE ' + QUOTENAME(@name) + N' SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE';
                    EXEC sp_executesql @alter;
                END
                """,
                connection);
            command.CommandTimeout = 120;
            command.Parameters.AddWithValue("@name", target.DatabaseName);

            await command.ExecuteNonQueryAsync(cancellationToken);
            log.Info($"READ_COMMITTED_SNAPSHOT checked on {target.DatabaseName}");
        }
        catch (SqlException ex)
        {
            log.Warn($"Could not set READ_COMMITTED_SNAPSHOT on {target.DatabaseName}: {ex.Message}");
        }
    }
}
