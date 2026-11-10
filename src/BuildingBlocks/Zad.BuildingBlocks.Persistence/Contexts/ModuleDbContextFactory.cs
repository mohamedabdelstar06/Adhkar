using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.BuildingBlocks.Persistence.Contexts;

public static class ModuleDbContextFactory
{
    public static ModuleDbContext Create(Type contextType, string connectionString, ITenantContext? tenantContext = null)
    {
        var builderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
        var builder = (DbContextOptionsBuilder)Activator.CreateInstance(builderType)!;

        var schema = ModuleDatabaseAttribute.For(contextType).Schema;
        Sql.ModuleSqlServer.Configure(builder, connectionString, schema);

        var optionsProperty = builderType.GetProperties()
            .First(property => property.Name == nameof(DbContextOptionsBuilder<ModuleDbContext>.Options) &&
                               property.DeclaringType == builderType);
        var options = optionsProperty.GetValue(builder)!;

        var context = Activator.CreateInstance(
            contextType,
            options,
            tenantContext ?? UnresolvedTenantContext.Instance);

        return (ModuleDbContext)context!;
    }

    public static ModuleDbContext Create<TContext>(string connectionString, ITenantContext? tenantContext = null)
        where TContext : ModuleDbContext =>
        Create(typeof(TContext), connectionString, tenantContext);
}
