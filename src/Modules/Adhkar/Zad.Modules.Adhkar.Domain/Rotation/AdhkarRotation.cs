using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Entities;

namespace Zad.Modules.Adhkar.Domain.Rotation;

public sealed class AdhkarRotation : TenantScopedAggregateRoot<RotationId>
{
    private AdhkarRotation()
    {
    }

    public Guid? CurrentAdhkarId { get; private set; }
    public DateTimeOffset? RotatedAtUtc { get; private set; }
    public int RotationCount { get; private set; }

    public static AdhkarRotation Start() =>
        new() { Id = RotationId.New(), CurrentAdhkarId = null, RotatedAtUtc = null, RotationCount = 0 };

    public Result MoveTo(Guid adhkarId, DateTimeOffset now)
    {
        CurrentAdhkarId = adhkarId;
        RotatedAtUtc = now;
        RotationCount++;

        return Result.Success();
    }
}
