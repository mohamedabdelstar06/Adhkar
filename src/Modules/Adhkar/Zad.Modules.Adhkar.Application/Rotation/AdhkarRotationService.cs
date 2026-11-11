using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Adhkar.Application.Adhkar;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using Zad.Modules.Adhkar.Domain.Adhkar;
using Zad.Modules.Adhkar.Domain.Rotation;

namespace Zad.Modules.Adhkar.Application.Rotation;

public sealed class AdhkarRotationService(
    IAdhkarRepository adhkarRepository,
    IAdhkarRotationRepository rotationRepository,
    IAdhkarUnitOfWork unitOfWork,
    TimeProvider timeProvider)
{
    private static readonly (string Text, string Translation, string Reference, string Category, int Order)[] DefaultAdhkar =
    [
        ("سُبْحَانَ اللَّهِ", "Glory be to Allah", "Muslim 2694", "tasbih", 1),
        ("الْحَمْدُ لِلَّهِ", "All praise is due to Allah", "Muslim 2694", "tahmid", 2),
        ("اللَّهُ أَكْبَرُ", "Allah is the Greatest", "Muslim 2694", "takbir", 3),
        ("لَا إِلَهَ إِلَّا اللَّهُ", "There is no god but Allah", "Muslim 2694", "tahlil", 4),
        ("لَا حَوْلَ وَلَا قُوَّةَ إِلَّا بِاللَّهِ", "There is no power and no strength except with Allah", "Bukhari 6384", "hawqala", 5),
        ("اللَّهُمَّ صَلِّ عَلَى مُحَمَّدٍ", "O Allah, send blessings upon Muhammad", "Bukhari 3370", "salawat", 6),
        ("سُبْحَانَ اللَّهِ وَبِحَمْدِهِ", "Glory be to Allah and praise be to Him", "Muslim 2691", "tasbih", 7),
        ("أَسْتَغْفِرُ اللَّهَ", "I seek forgiveness from Allah", "Muslim 2702", "tahlil", 8)
    ];

    public async Task<Result<AdhkarDto>> RotateAsync(CancellationToken cancellationToken = default)
    {
        var active = await adhkarRepository.ListActiveOrderedAsync(cancellationToken);
        if (active.Count == 0)
        {
            await SeedDefaultsAsync(cancellationToken);
            active = await adhkarRepository.ListActiveOrderedAsync(cancellationToken);
            if (active.Count == 0)
            {
                return Result<AdhkarDto>.Failure(AdhkarErrors.NothingToRotate);
            }
        }

        var rotation = await rotationRepository.GetAsync(cancellationToken);
        if (rotation is null)
        {
            rotation = AdhkarRotation.Start();
            rotationRepository.Add(rotation);
        }

        var next = Next(active, rotation.CurrentAdhkarId);
        rotation.MoveTo(next.Id.Value, timeProvider.GetUtcNow());

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        return saveResult.IsSuccess
            ? Result<AdhkarDto>.Success(next.ToDto())
            : Result<AdhkarDto>.Failure(saveResult.Error);
    }

    private async Task SeedDefaultsAsync(CancellationToken cancellationToken)
    {
        foreach (var item in DefaultAdhkar)
        {
            var createResult = Domain.Adhkar.Adhkar.Create(
                item.Text, item.Translation, item.Reference, item.Category, item.Order, true);
            if (createResult.IsSuccess)
            {
                adhkarRepository.Add(createResult.Value);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Domain.Adhkar.Adhkar Next(IReadOnlyList<Domain.Adhkar.Adhkar> active, Guid? currentId)
    {
        if (currentId is null)
        {
            return active[0];
        }

        var index = -1;
        for (var i = 0; i < active.Count; i++)
        {
            if (active[i].Id.Value == currentId.Value)
            {
                index = i;
                break;
            }
        }

        return index < 0 ? active[0] : active[(index + 1) % active.Count];
    }
}
