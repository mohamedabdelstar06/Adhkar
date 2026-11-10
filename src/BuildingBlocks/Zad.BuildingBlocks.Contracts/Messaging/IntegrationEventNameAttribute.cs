namespace Zad.BuildingBlocks.Contracts.Messaging;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IntegrationEventNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

