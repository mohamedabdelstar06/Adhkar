using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Domain.Entities;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Persistence.Configuration;

public abstract class TenantScopedEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : TenantScopedEntity<TId>
    where TId : struct, IEntityId
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => new { entity.TenantId, entity.Id }).IsClustered(false);

        builder.Property(entity => entity.Id).ValueGeneratedNever();

        builder.Property<long>(ClusteringColumnNames.SequenceNumber).UseIdentityColumn();

        builder.HasIndex(nameof(TenantScopedEntity<TId>.TenantId), ClusteringColumnNames.SequenceNumber)
            .IsUnique()
            .IsClustered()
            .HasDatabaseName($"CX_{typeof(TEntity).Name}_{ClusteringColumnNames.SequenceNumber}");

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
