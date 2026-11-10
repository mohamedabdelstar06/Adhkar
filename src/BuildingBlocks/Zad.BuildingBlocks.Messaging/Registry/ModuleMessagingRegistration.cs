namespace Zad.BuildingBlocks.Messaging.Registry;

public sealed record ModuleMessagingRegistration(
    string Module,
    string Schema,
    Type ContextType,
    Type UnitOfWorkType);

