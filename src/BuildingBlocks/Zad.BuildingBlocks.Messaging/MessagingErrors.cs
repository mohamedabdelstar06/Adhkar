using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Messaging;

public static class MessagingErrors
{
    public static readonly Error LeaseLost =
        Error.Conflict("messaging.lease-lost", "The message lease expired and another processor owns the row");

    public static readonly Error HandlerMissing =
        Error.Failure("messaging.handler-missing", "No handler is registered for the inbox row");

    public static readonly Error UnknownEvent =
        Error.Failure("messaging.unknown-event", "The message names an integration event that is not registered");
}
