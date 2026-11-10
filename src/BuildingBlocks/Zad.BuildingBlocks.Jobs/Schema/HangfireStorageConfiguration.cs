using Hangfire.SqlServer;

namespace Zad.BuildingBlocks.Jobs.Schema;

public static class HangfireStorageConfiguration
{
    public const string SchemaName = "hangfire";

    public static SqlServerStorageOptions StorageOptions() => new()
    {
        SchemaName = SchemaName,
        PrepareSchemaIfNecessary = false,
        QueuePollInterval = TimeSpan.FromSeconds(15),
        DisableGlobalLocks = true
    };
}

