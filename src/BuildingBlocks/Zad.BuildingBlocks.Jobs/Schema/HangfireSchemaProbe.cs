using Microsoft.Data.SqlClient;

namespace Zad.BuildingBlocks.Jobs.Schema;

public sealed class HangfireSchemaProbe : Persistence.Migrations.IHangfireSchemaProbe
{
    public async Task<bool> IsInstalledAsync(string jobsConnectionString, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(jobsConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id " +
            "WHERE s.name = @schema AND t.name = 'Schema'";
        command.Parameters.AddWithValue("@schema", "hangfire");

        var count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
        return count > 0;
    }
}
