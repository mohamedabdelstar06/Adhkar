using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zad.BuildingBlocks.Persistence.Configuration;
using Zad.Modules.Tenancy.Domain.Tenants;

namespace Zad.Modules.Tenancy.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : PlatformEntityConfiguration<Tenant, TenantId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", TenancyDbContext.SchemaName);

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => TenantId.From(value));

        builder.Property(t => t.Slug).HasMaxLength(63).IsRequired();
        builder.Property(t => t.Host).HasMaxLength(255).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.DefaultLanguage).HasMaxLength(8).IsRequired();
        builder.Property(t => t.EnabledLanguages).HasMaxLength(64).IsRequired();
        builder.Property(t => t.TimeZone).HasMaxLength(64).IsRequired();

        builder.HasIndex(t => t.Slug).IsUnique().HasDatabaseName("UX_Tenants_Slug");
        builder.HasIndex(t => t.Host).IsUnique().HasDatabaseName("UX_Tenants_Host");
    }
}
