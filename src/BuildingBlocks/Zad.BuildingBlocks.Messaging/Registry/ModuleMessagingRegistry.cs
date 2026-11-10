namespace Zad.BuildingBlocks.Messaging.Registry;

public sealed class ModuleMessagingRegistry
{
    private readonly Dictionary<string, ModuleMessagingRegistration> _modules = new(StringComparer.OrdinalIgnoreCase);

    public ModuleMessagingRegistry(IEnumerable<ModuleMessagingRegistration> registrations)
    {
        foreach (var registration in registrations)
        {
            if (!_modules.TryAdd(registration.Module, registration))
            {
                throw new InvalidOperationException($"Module '{registration.Module}' registered messaging twice");
            }
        }
    }

    public IReadOnlyCollection<ModuleMessagingRegistration> All => _modules.Values;

    public ModuleMessagingRegistration? Find(string module) =>
        _modules.GetValueOrDefault(module);
}

