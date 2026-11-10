namespace Zad.BuildingBlocks.Persistence.Contexts;

public sealed record ModuleDbContextDescriptor(
    Type ContextType,
    string Module,
    ModuleDatabaseKind Kind,
    string Schema)
{
    public static ModuleDbContextDescriptor For(Type contextType)
    {
        var attribute = ModuleDatabaseAttribute.For(contextType);
        return new ModuleDbContextDescriptor(contextType, ModuleNameOf(contextType), attribute.Kind, attribute.Schema);
    }

    private static string ModuleNameOf(Type contextType)
    {
        var assemblyName = contextType.Assembly.GetName().Name ?? string.Empty;

        const string modulePrefix = "Zad.Modules.";
        const string moduleSuffix = ".Infrastructure";
        if (assemblyName.StartsWith(modulePrefix, StringComparison.Ordinal) &&
            assemblyName.EndsWith(moduleSuffix, StringComparison.Ordinal))
        {
            return assemblyName[modulePrefix.Length..^moduleSuffix.Length];
        }

        const string blockPrefix = "Zad.BuildingBlocks.";
        if (assemblyName.StartsWith(blockPrefix, StringComparison.Ordinal))
        {
            return assemblyName[blockPrefix.Length..];
        }

        return assemblyName;
    }
}
