namespace Zad.BuildingBlocks.Messaging.Outbox;

public enum MessageStatus
{
    Pending,
    Dispatched,
    Processed,
    DeadLettered
}

