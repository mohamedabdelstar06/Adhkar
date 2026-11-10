using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.BuildingBlocks.Messaging.Registry;

public static class MessagingContextFactory
{
    public static Func<IServiceProvider, Guid, DbContext> For(Type contextType)
    {
        var factoryType = typeof(IModuleDbContextFactory<>).MakeGenericType(contextType);
        var create = factoryType.GetMethod(nameof(IModuleDbContextFactory<ModuleDbContext>.CreateForDatabase))!;

        return (provider, databaseId) =>
            (DbContext)create.Invoke(provider.GetRequiredService(factoryType), [databaseId])!;
    }
}
