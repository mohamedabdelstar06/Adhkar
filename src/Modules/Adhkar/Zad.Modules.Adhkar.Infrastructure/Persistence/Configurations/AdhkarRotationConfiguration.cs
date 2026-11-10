using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Persistence.Configuration;
using AdhkarRotationEntity = Zad.Modules.Adhkar.Domain.Rotation.AdhkarRotation;
using RotationId = Zad.Modules.Adhkar.Domain.Rotation.RotationId;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence.Configurations;

public sealed class AdhkarRotationConfiguration : TenantScopedEntityConfiguration<AdhkarRotationEntity, RotationId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<AdhkarRotationEntity> builder)
    {
        builder.ToTable("Rotations", AdhkarDbContext.SchemaName);

        builder.Property(rotation => rotation.Id)
            .HasConversion(id => id.Value, value => RotationId.From(value));

        builder.HasIndex(rotation => rotation.TenantId)
            .IsUnique()
            .HasDatabaseName("UX_Rotations_TenantId");
    }
}

