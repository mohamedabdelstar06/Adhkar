using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Zad.BuildingBlocks.Domain.Tenancy;

namespace Zad.BuildingBlocks.Persistence.Tenancy;

public static class TenantModelRules
{
    public static void Validate(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned())
            {
                continue;
            }

            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            ValidateIndexes(entityType);
        }
    }

    private static void ValidateIndexes(IMutableEntityType entityType)
    {
        foreach (var index in entityType.GetIndexes())
        {
            var first = index.Properties.Count == 0 ? null : index.Properties[0];

            if (first is null ||
                !string.Equals(first.Name, nameof(ITenantScoped.TenantId), StringComparison.Ordinal))
            {
                var name = index.GetDatabaseName() ?? string.Join(", ", index.Properties.Select(property => property.Name));

                throw new InvalidOperationException(
                    $"Index '{name}' on '{entityType.DisplayName()}' must lead with TenantId (R34).");
            }
        }
    }
}
