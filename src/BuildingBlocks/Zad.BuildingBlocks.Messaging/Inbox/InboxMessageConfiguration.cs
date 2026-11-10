using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zad.BuildingBlocks.Messaging.Inbox;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.HandlerName).HasMaxLength(300).IsRequired();
        builder.Property(message => message.EventName).HasMaxLength(200).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(2000);
        builder.Property(message => message.LockedBy).HasMaxLength(100);

        builder.HasIndex(message => new { message.MessageId, message.HandlerName })
            .IsUnique()
            .HasDatabaseName("UX_InboxMessages_MessageId_HandlerName");

        builder.HasIndex(message => message.NextAttemptAtUtc)
            .HasDatabaseName("IX_InboxMessages_Pending")
            .HasFilter("[Status] = 'Pending'");

        builder.HasIndex(message => message.ProcessedAtUtc)
            .HasDatabaseName("IX_InboxMessages_Processed")
            .HasFilter("[Status] = 'Processed'");
    }
}

