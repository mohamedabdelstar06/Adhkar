using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Persistence.Reads;
using Zad.Modules.Adhkar.Application.Adhkar;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using AdhkarEntity = Zad.Modules.Adhkar.Domain.Adhkar.Adhkar;
using AdhkarId = Zad.Modules.Adhkar.Domain.Adhkar.AdhkarId;
using AdhkarRotationEntity = Zad.Modules.Adhkar.Domain.Rotation.AdhkarRotation;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence.ReadRepositories;

public sealed class AdhkarReadRepository(IReadOnlyDbQuery<AdhkarDbContext> query) : IAdhkarReadRepository
{
    public async Task<IReadOnlyList<AdhkarDto>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var baseQuery = query.Query<AdhkarEntity>();

        if (!includeInactive)
        {
            baseQuery = baseQuery.Where(adhkar => adhkar.IsActive);
        }

        return await baseQuery
            .OrderBy(adhkar => adhkar.DisplayOrder)
            .Select(adhkar => new AdhkarDto(
                adhkar.Id.Value,
                adhkar.Text,
                adhkar.Translation,
                adhkar.Reference,
                adhkar.Category,
                adhkar.DisplayOrder,
                adhkar.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<AdhkarDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var targetId = AdhkarId.From(id);

        return await query.Query<AdhkarEntity>()
            .Where(adhkar => adhkar.Id == targetId)
            .Select(adhkar => new AdhkarDto(
                adhkar.Id.Value,
                adhkar.Text,
                adhkar.Translation,
                adhkar.Reference,
                adhkar.Category,
                adhkar.DisplayOrder,
                adhkar.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdhkarDto?> GetFirstActiveAsync(CancellationToken cancellationToken = default) =>
        await query.Query<AdhkarEntity>()
            .Where(adhkar => adhkar.IsActive)
            .OrderBy(adhkar => adhkar.DisplayOrder)
            .Select(adhkar => new AdhkarDto(
                adhkar.Id.Value,
                adhkar.Text,
                adhkar.Translation,
                adhkar.Reference,
                adhkar.Category,
                adhkar.DisplayOrder,
                adhkar.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Guid?> GetRotationCurrentIdAsync(CancellationToken cancellationToken = default) =>
        await query.Query<AdhkarRotationEntity>()
            .Select(rotation => rotation.CurrentAdhkarId)
            .FirstOrDefaultAsync(cancellationToken);
}
