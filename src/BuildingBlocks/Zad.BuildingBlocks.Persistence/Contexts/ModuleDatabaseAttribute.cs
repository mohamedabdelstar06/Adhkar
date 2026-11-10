namespace Zad.BuildingBlocks.Persistence.Contexts;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ModuleDatabaseAttribute(ModuleDatabaseKind kind, string schema) : Attribute
{
    public ModuleDatabaseKind Kind { get; } = kind;

    public string Schema { get; } = schema;

    public static ModuleDatabaseAttribute For(Type contextType) =>
        contextType.GetCustomAttributes(typeof(ModuleDatabaseAttribute), inherit: false)
            .OfType<ModuleDatabaseAttribute>()
            .FirstOrDefault()
        ?? throw new InvalidOperationException(
            $"DbContext '{contextType.FullName}' must be annotated with [ModuleDatabase]");
}
