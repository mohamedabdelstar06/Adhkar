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
        ("Ø³ÙØ¨Ù’Ø­ÙŽØ§Ù†ÙŽ Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù", "Glory be to Allah", "Muslim 2694", "tasbih", 1),
        ("Ø§Ù„Ù’Ø­ÙŽÙ…Ù’Ø¯Ù Ù„ÙÙ„ÙŽÙ‘Ù‡Ù", "All praise is due to Allah", "Muslim 2694", "tahmid", 2),
        ("Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù Ø£ÙŽÙƒÙ’Ø¨ÙŽØ±Ù", "Allah is the Greatest", "Muslim 2694", "takbir", 3),
        ("Ù„ÙŽØ§ Ø¥ÙÙ„ÙŽÙ‡ÙŽ Ø¥ÙÙ„ÙŽÙ‘Ø§ Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù", "There is no god but Allah", "Muslim 2694", "tahlil", 4),
        ("Ù„ÙŽØ§ Ø­ÙŽÙˆÙ’Ù„ÙŽ ÙˆÙŽÙ„ÙŽØ§ Ù‚ÙÙˆÙŽÙ‘Ø©ÙŽ Ø¥ÙÙ„ÙŽÙ‘Ø§ Ø¨ÙØ§Ù„Ù„ÙŽÙ‘Ù‡Ù", "There is no power and no strength except with Allah", "Bukhari 6384", "hawqala", 5),
        ("Ø§Ù„Ù„ÙŽÙ‘Ù‡ÙÙ…ÙŽÙ‘ ØµÙŽÙ„ÙÙ‘ Ø¹ÙŽÙ„ÙŽÙ‰ Ù…ÙØ­ÙŽÙ…ÙŽÙ‘Ø¯Ù", "O Allah, send blessings upon Muhammad", "Bukhari 3370", "salawat", 6),
        ("Ø³ÙØ¨Ù’Ø­ÙŽØ§Ù†ÙŽ Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù ÙˆÙŽØ¨ÙØ­ÙŽÙ…Ù’Ø¯ÙÙ‡Ù", "Glory be to Allah and praise be to Him", "Muslim 2691", "tasbih", 7),
        ("Ø£ÙŽØ³Ù’ØªÙŽØºÙ’ÙÙØ±Ù Ø§Ù„Ù„ÙŽÙ‘Ù‡ÙŽ", "I seek forgiveness from Allah", "Muslim 2702", "tahlil", 8)
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
