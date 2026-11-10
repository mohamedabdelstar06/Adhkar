using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zad.BuildingBlocks.Messaging.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.EventName).HasMaxLength(200).IsRequired();
        builder.Property(message => message.Payload).IsRequired();
        builder.Property(message => message.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(2000);
        builder.Property(message => message.LockedBy).HasMaxLength(100);

        builder.HasIndex(message => message.NextAttemptAtUtc)
            .HasDatabaseName("IX_OutboxMessages_Pending")
            .HasFilter("[Status] = 'Pending'");

        builder.HasIndex(message => message.DispatchedAtUtc)
            .HasDatabaseName("IX_OutboxMessages_Dispatched")
            .HasFilter("[Status] = 'Dispatched'");
    }
}

