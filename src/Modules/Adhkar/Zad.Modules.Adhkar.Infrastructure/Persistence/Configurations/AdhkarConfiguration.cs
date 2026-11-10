using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Persistence.Configuration;
using AdhkarEntity = Zad.Modules.Adhkar.Domain.Adhkar.Adhkar;
using AdhkarId = Zad.Modules.Adhkar.Domain.Adhkar.AdhkarId;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence.Configurations;

public sealed class AdhkarConfiguration : TenantScopedEntityConfiguration<AdhkarEntity, AdhkarId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<AdhkarEntity> builder)
    {
        builder.ToTable("Adhkar", AdhkarDbContext.SchemaName);

        builder.Property(adhkar => adhkar.Id)
            .HasConversion(id => id.Value, value => AdhkarId.From(value));

        builder.Property(adhkar => adhkar.Text).HasMaxLength(1000).IsRequired();
        builder.Property(adhkar => adhkar.Translation).HasMaxLength(2000).IsRequired();
        builder.Property(adhkar => adhkar.Reference).HasMaxLength(300).IsRequired();
        builder.Property(adhkar => adhkar.Category).HasMaxLength(32).IsRequired();

        builder.HasIndex(adhkar => new { adhkar.TenantId, adhkar.DisplayOrder })
            .IsUnique()
            .HasDatabaseName("UX_Adhkar_TenantId_DisplayOrder");
    }
}
