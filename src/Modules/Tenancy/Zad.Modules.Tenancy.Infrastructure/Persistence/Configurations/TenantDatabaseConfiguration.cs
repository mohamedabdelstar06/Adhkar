using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Persistence.Configuration;
using Zad.Modules.Tenancy.Domain.Databases;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence.Configurations;

public sealed class TenantDatabaseConfiguration : PlatformEntityConfiguration<TenantDatabase, DatabaseId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TenantDatabase> builder)
    {
        builder.ToTable("Databases", TenancyDbContext.SchemaName);

        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, value => DatabaseId.From(value));

        builder.Property(d => d.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(128).IsRequired();
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(d => d.AppConnectionStringProtected).HasMaxLength(4000).IsRequired();
        builder.Property(d => d.MigratorConnectionStringProtected).HasMaxLength(4000).IsRequired();

        builder.HasIndex(d => d.Name).IsUnique().HasDatabaseName("UX_Databases_Name");
    }
}

