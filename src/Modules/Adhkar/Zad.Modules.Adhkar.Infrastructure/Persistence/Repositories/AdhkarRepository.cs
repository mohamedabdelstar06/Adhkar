using Microsoft.EntityFrameworkCore;
using AdhkarEntity = Zad.Modules.Adhkar.Domain.Adhkar.Adhkar;
using AdhkarId = Zad.Modules.Adhkar.Domain.Adhkar.AdhkarId;
using IAdhkarRepository = Zad.Modules.Adhkar.Domain.Adhkar.IAdhkarRepository;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence.Repositories;

public sealed class AdhkarRepository(AdhkarDbContext context) : IAdhkarRepository
{
    public Task<AdhkarEntity?> FindAsync(AdhkarId id, CancellationToken cancellationToken = default) =>
        context.Adhkar.FirstOrDefaultAsync(adhkar => adhkar.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AdhkarEntity>> ListActiveOrderedAsync(CancellationToken cancellationToken = default) =>
        await context.Adhkar
            .Where(adhkar => adhkar.IsActive)
            .OrderBy(adhkar => adhkar.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<bool> DisplayOrderTakenAsync(int displayOrder, AdhkarId? excluded, CancellationToken cancellationToken = default)
    {
        var query = context.Adhkar.Where(adhkar => adhkar.DisplayOrder == displayOrder);

        if (excluded is { } excludedId)
        {
            query = query.Where(adhkar => adhkar.Id != excludedId);
        }

        return query.AnyAsync(cancellationToken);
    }

    public void Add(AdhkarEntity adhkar) => context.Adhkar.Add(adhkar);

    public void Remove(AdhkarEntity adhkar) => context.Adhkar.Remove(adhkar);
}
