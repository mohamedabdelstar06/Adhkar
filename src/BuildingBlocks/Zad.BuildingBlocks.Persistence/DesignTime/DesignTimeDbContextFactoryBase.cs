using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Zad.BuildingBlocks.Persistence.Contexts;
using Zad.BuildingBlocks.Persistence.Sql;

namespace Zad.BuildingBlocks.Persistence.DesignTime;

public abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : ModuleDbContext
{
    private const string DefaultDesignConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=ZadDesign;Trusted_Connection=True;TrustServerCertificate=True;";

    protected abstract TContext Create(DbContextOptions<TContext> options);

    public TContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ZAD_MIGRATIONS_DESIGN_CONNECTION_STRING")
            ?? DefaultDesignConnectionString;

        var builder = new DbContextOptionsBuilder<TContext>();
        ModuleSqlServer.Configure(builder, connectionString, ModuleDatabaseAttribute.For(typeof(TContext)).Schema);

        return Create(builder.Options);
    }
}
