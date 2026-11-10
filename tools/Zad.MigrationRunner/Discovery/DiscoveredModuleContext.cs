using Zad.BuildingBlocks.Persistence.Contexts;

namespace Zad.MigrationRunner.Discovery;

public sealed record DiscoveredModuleContext(
    string ModuleName,
    Type ContextType,
    ModuleDatabaseKind Kind,
    string Schema);

