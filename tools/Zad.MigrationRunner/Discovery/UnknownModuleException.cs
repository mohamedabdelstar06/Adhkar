namespace Zad.MigrationRunner.Discovery;

public sealed class UnknownModuleException(string moduleName)
    : Exception($"The context module '{moduleName}' is not in ModuleMigrationOrder.Modules")
{
    public string ModuleName { get; } = moduleName;
}

