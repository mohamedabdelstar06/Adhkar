using Microsoft.EntityFrameworkCore;

namespace Zad.BuildingBlocks.Persistence.Sql;

public static class ModuleSqlServer
{
    public static void Configure(DbContextOptionsBuilder options, string connectionString, string schema)
    {
        options.UseSqlServer(connectionString, sql => sql
            .MigrationsHistoryTable("__EFMigrationsHistory", schema)
            .EnableRetryOnFailure(3));
    }
}
