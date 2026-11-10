using Microsoft.Data.SqlClient;
using Zad.MigrationRunner.Targets;

namespace Zad.MigrationRunner.Journal;

public sealed class MigrationJournal
{
    private const string CreateSql = """
        IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'migrations')
            EXEC('CREATE SCHEMA migrations');

        IF NOT EXISTS (SELECT 1 FROM sys.tables
                       WHERE name = 'AppliedMigrations' AND schema_id = SCHEMA_ID('migrations'))
            CREATE TABLE migrations.AppliedMigrations
            (
                DatabaseId    uniqueidentifier NOT NULL,
                Module        nvarchar(64)     NOT NULL,
                Version       nvarchar(200)    NOT NULL,
                AppliedOnUtc  datetime2(3)     NOT NULL,
                DurationMs    int              NOT NULL,
                Status        nvarchar(32)     NOT NULL,
                Error         nvarchar(max)    NULL,
                RunnerVersion nvarchar(64)     NOT NULL,
                CONSTRAINT PK_AppliedMigrations PRIMARY KEY (DatabaseId, Module, Version)
            );
        """;

    private const string UpsertSql = """
        MERGE migrations.AppliedMigrations AS target
        USING (SELECT @DatabaseId AS DatabaseId, @Module AS Module, @Version AS Version) AS source
            ON target.DatabaseId = source.DatabaseId
               AND target.Module = source.Module
               AND target.Version = source.Version
        WHEN MATCHED THEN
            UPDATE SET AppliedOnUtc = SYSUTCDATETIME(),
                       DurationMs = @DurationMs,
                       Status = @Status,
                       Error = @Error,
                       RunnerVersion = @RunnerVersion
        WHEN NOT MATCHED THEN
            INSERT (DatabaseId, Module, Version, AppliedOnUtc, DurationMs, Status, Error, RunnerVersion)
            VALUES (@DatabaseId, @Module, @Version, SYSUTCDATETIME(), @DurationMs, @Status, @Error, @RunnerVersion);
        """;

    public async Task EnsureCreatedAsync(MigrationTarget target, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(target.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(CreateSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task RecordAppliedAsync(
        MigrationTarget target,
        string module,
        IEnumerable<string> versions,
        long durationMs,
        CancellationToken cancellationToken)
    {
        foreach (var version in versions)
        {
            await WriteAsync(target, module, version, durationMs, "Applied", null, cancellationToken);
        }
    }

    public async Task RecordFailedAsync(
        MigrationTarget target,
        string module,
        IEnumerable<string> versions,
        string error,
        CancellationToken cancellationToken)
    {
        await WriteAsync(target, module, string.Join(',', versions), 0, "Failed", error, cancellationToken);
    }

    private static async Task WriteAsync(
        MigrationTarget target,
        string module,
        string version,
        long durationMs,
        string status,
        string? error,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(target.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(UpsertSql, connection);
        command.Parameters.AddWithValue("@DatabaseId", target.DatabaseId);
        command.Parameters.AddWithValue("@Module", module);
        command.Parameters.AddWithValue("@Version", Truncate(version, 200));
        command.Parameters.AddWithValue("@DurationMs", (int)durationMs);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@Error", (object?)error ?? DBNull.Value);
        command.Parameters.AddWithValue("@RunnerVersion", typeof(MigrationJournal).Assembly.GetName().Version?.ToString() ?? "0.0.0");

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
