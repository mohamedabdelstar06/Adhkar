using Microsoft.EntityFrameworkCore;
using AdhkarRotationEntity = Zad.Modules.Adhkar.Domain.Rotation.AdhkarRotation;
using IAdhkarRotationRepository = Zad.Modules.Adhkar.Domain.Rotation.IAdhkarRotationRepository;

namespace Zad.Modules.Adhkar.Infrastructure.Persistence.Repositories;

public sealed class AdhkarRotationRepository(AdhkarDbContext context) : IAdhkarRotationRepository
{
    public Task<AdhkarRotationEntity?> GetAsync(CancellationToken cancellationToken = default) =>
        context.Rotations.FirstOrDefaultAsync(cancellationToken);

    public void Add(AdhkarRotationEntity rotation) => context.Rotations.Add(rotation);
}

