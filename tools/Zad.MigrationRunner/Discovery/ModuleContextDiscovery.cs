using System.Reflection;
using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.MigrationRunner.Discovery;

public sealed class ModuleContextDiscovery
{
    private static readonly string[] Patterns =
    [
        "Zad.Modules.*.Infrastructure.dll",
        "Zad.BuildingBlocks.Persistence.dll"
    ];

    public IReadOnlyList<DiscoveredModuleContext> Discover()
    {
        var discovered = Patterns
            .SelectMany(pattern => Directory.EnumerateFiles(AppContext.BaseDirectory, pattern))
            .Select(Assembly.LoadFrom)
            .SelectMany(assembly => assembly.GetTypes()
                .Where(type => type is { IsAbstract: false, IsPublic: true } && typeof(ModuleDbContext).IsAssignableFrom(type))
                .Select(type => new DiscoveredModuleContext(
                    ModuleDbContextDescriptor.For(type).Module,
                    type,
                    ModuleDatabaseAttribute.For(type).Kind,
                    ModuleDatabaseAttribute.For(type).Schema)))
            .ToList();

        var unknown = discovered
            .Select(context => context.ModuleName)
            .FirstOrDefault(module => !ModuleMigrationOrder.Modules.Contains(module, StringComparer.Ordinal));

        return unknown is null ? discovered : throw new UnknownModuleException(unknown);
    }
}
