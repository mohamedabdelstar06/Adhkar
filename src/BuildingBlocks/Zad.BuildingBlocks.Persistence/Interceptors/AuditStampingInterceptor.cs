using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Zad.BuildingBlocks.Application.Identity;
using Zad.BuildingBlocks.Domain.Entities;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Persistence.Interceptors;

public sealed class AuditStampingInterceptor(TimeProvider timeProvider, ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        StampAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAtUtc") is { ClrType: var createdClrType } &&
                    createdClrType == typeof(DateTimeOffset))
                {
                    entry.Property("CreatedAtUtc").CurrentValue = now;
                }

                if (entry.Metadata.FindProperty("CreatedBy") is not null)
                {
                    entry.Property("CreatedBy").CurrentValue = userId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Metadata.FindProperty("ModifiedAtUtc") is { ClrType: var modifiedClrType } &&
                    modifiedClrType == typeof(DateTimeOffset?))
                {
                    entry.Property("ModifiedAtUtc").CurrentValue = now;
                }

                if (entry.Metadata.FindProperty("ModifiedBy") is not null)
                {
                    entry.Property("ModifiedBy").CurrentValue = userId;
                }
            }

            if (entry.Entity is IAggregateRoot &&
                entry.State is EntityState.Added or EntityState.Modified &&
                entry.Metadata.FindProperty("RowVersion") is not null)
            {
                entry.Property("RowVersion").CurrentValue = GuidV7.New().ToByteArray();
            }
        }
    }
}

