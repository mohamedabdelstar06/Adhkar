using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Jobs.Sweeps;
using Zad.BuildingBlocks.Jobs.Tenancy;
using Zad.Modules.Adhkar.Application.Rotation;
using Zad.Modules.Adhkar.Contracts;

namespace Zad.Modules.Adhkar.Infrastructure.Adhkar;

public sealed class AdhkarRotationJob(
    AdhkarRotationService rotationService,
    ILogger<AdhkarRotationJob> logger) : ITenantJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var result = await rotationService.RotateAsync(cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogDebug("Adhkar rotation moved to {AdhkarId}", result.Value.Id);
            return;
        }

        if (result.Error.Code == AdhkarErrorCodes.NothingToRotate)
        {
            logger.LogDebug("Adhkar rotation skipped because this tenant has no active adhkar");
            return;
        }

        throw new JobFailedException(result.Error);
    }
}
