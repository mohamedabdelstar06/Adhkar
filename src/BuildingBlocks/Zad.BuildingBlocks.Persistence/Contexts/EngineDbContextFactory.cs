using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Platform;
using Zad.BuildingBlocks.Persistence.Sql;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.BuildingBlocks.Persistence.Contexts;

public sealed class EngineDbContextFactory<TContext>(
    IServiceProvider provider,
    ITenantConnectionResolver connections,
    IDatabaseRegistry databases,
    IConfiguration configuration) : IModuleDbContextFactory<TContext>
    where TContext : ModuleDbContext
{
    public TContext CreateForDatabase(Guid databaseId)
    {
        if (databases.Find(databaseId) is null)
        {
            throw new InvalidOperationException($"Database '{databaseId}' is not registered in the catalog");
        }

        return Create(connections.ResolveApplicationConnectionStringForDatabase(databaseId));
    }

    public TContext CreateForCatalog()
    {
        var connectionString = configuration.GetConnectionString(PlatformConnectionStrings.Catalog);

        return string.IsNullOrWhiteSpace(connectionString)
            ? throw new InvalidOperationException("ConnectionStrings:Catalog is required to open a catalog context")
            : Create(connectionString);
    }

    private TContext Create(string connectionString)
    {
        var descriptor = ModuleDbContextDescriptor.For(typeof(TContext));
        var builder = new DbContextOptionsBuilder<TContext>();

        ModuleSqlServer.Configure(builder, connectionString, descriptor.Schema);

        return ActivatorUtilities.CreateInstance<TContext>(
            provider,
            builder.Options,
            UnresolvedTenantContext.Instance);
    }
}
