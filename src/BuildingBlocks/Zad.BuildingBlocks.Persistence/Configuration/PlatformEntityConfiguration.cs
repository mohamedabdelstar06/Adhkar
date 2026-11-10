using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Domain.Entities;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Persistence.Configuration;

public abstract class PlatformEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity<TId>
    where TId : struct, IEntityId
{
    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id).IsClustered(false);

        builder.Property(entity => entity.Id).ValueGeneratedNever();

        builder.Property<long>(ClusteringColumnNames.SequenceNumber).UseIdentityColumn();

        builder.HasIndex(ClusteringColumnNames.SequenceNumber)
            .IsUnique()
            .IsClustered()
            .HasDatabaseName($"CX_{typeof(TEntity).Name}_{ClusteringColumnNames.SequenceNumber}");

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}

