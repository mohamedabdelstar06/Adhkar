using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Domain.Entities;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Domain.SoftDeletion;
using Zad.BuildingBlocks.Domain.Tenancy;
using Zad.BuildingBlocks.Persistence.Tenancy;

namespace Zad.BuildingBlocks.Persistence.Contexts;

public abstract class ModuleDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    protected ModuleDbContext(DbContextOptions options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    protected ModuleDbContext(DbContextOptions options)
        : this(options, UnresolvedTenantContext.Instance)
    {
    }

    protected abstract string Schema { get; }

    public Guid CurrentTenantId => _tenantContext.IsResolved
        ? _tenantContext.TenantId
        : throw new InvalidOperationException("Tenant context is required when querying tenant-scoped data");

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        ConfigureEngineTables(modelBuilder);

        ApplyTenantIsolationFilter(modelBuilder);
        ApplySoftDeleteFilter(modelBuilder);
        ApplyConcurrencyTokens(modelBuilder);

        TenantModelRules.Validate(modelBuilder);
    }

    protected virtual void ConfigureEngineTables(ModelBuilder modelBuilder)
    {
    }

    private void ApplyTenantIsolationFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(ITenantScoped).IsAssignableFrom(clrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "entity");
            var tenantId = Expression.Property(parameter, nameof(ITenantScoped.TenantId));
            var current = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
            var filter = Expression.Lambda(Expression.Equal(tenantId, current), parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(ModuleFilterNames.TenantIsolation, filter);
        }
    }

    private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (!typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "entity");
            var isDeleted = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Equal(isDeleted, Expression.Constant(false)), parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(ModuleFilterNames.SoftDelete, filter);
        }
    }

    private static void ApplyConcurrencyTokens(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IAggregateRoot).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var rowVersion = entityType.FindProperty(nameof(AggregateRoot<PlaceholderId>.RowVersion));

            if (rowVersion is not null)
            {
                rowVersion.IsConcurrencyToken = true;
            }
        }
    }

    private readonly record struct PlaceholderId(Guid Value) : IEntityId;
}
