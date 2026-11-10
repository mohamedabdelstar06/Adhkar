using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Messaging.Inbox;
using Zad.BuildingBlocks.Messaging.Outbox;

namespace Zad.BuildingBlocks.Messaging;

public static class MessagingModelExtensions
{
    public static ModelBuilder AddMessagingTables(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());

        return modelBuilder;
    }
}
