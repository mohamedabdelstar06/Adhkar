using Zad.BuildingBlocks.Contracts.Results;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using Zad.Modules.Adhkar.Domain.Adhkar;

namespace Zad.Modules.Adhkar.Application.Adhkar;

public sealed class AdhkarManagementService(
    IAdhkarRepository adhkarRepository,
    IAdhkarUnitOfWork unitOfWork)
{
    public async Task<Result<AdhkarDto>> CreateAsync(CreateAdhkarRequest request, CancellationToken cancellationToken = default)
    {
        if (await adhkarRepository.DisplayOrderTakenAsync(request.DisplayOrder, null, cancellationToken))
        {
            return Result<AdhkarDto>.Failure(AdhkarErrors.DuplicateOrder);
        }

        var createResult = Domain.Adhkar.Adhkar.Create(
            request.Text,
            request.Translation,
            request.Reference,
            request.Category,
            request.DisplayOrder,
            request.IsActive);

        if (createResult.IsFailure)
        {
            return Result<AdhkarDto>.Failure(createResult.Error);
        }

        adhkarRepository.Add(createResult.Value);
        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        return saveResult.IsSuccess
            ? Result<AdhkarDto>.Success(createResult.Value.ToDto())
            : Result<AdhkarDto>.Failure(saveResult.Error);
    }

    public async Task<Result<AdhkarDto>> UpdateAsync(Guid id, UpdateAdhkarRequest request, CancellationToken cancellationToken = default)
    {
        var adhkar = await adhkarRepository.FindAsync(AdhkarId.From(id), cancellationToken);
        if (adhkar is null)
        {
            return Result<AdhkarDto>.Failure(AdhkarErrors.NotFound);
        }

        if (await adhkarRepository.DisplayOrderTakenAsync(request.DisplayOrder, adhkar.Id, cancellationToken))
        {
            return Result<AdhkarDto>.Failure(AdhkarErrors.DuplicateOrder);
        }

        var updateResult = adhkar.Update(
            request.Text,
            request.Translation,
            request.Reference,
            request.Category,
            request.DisplayOrder,
            request.IsActive);

        if (updateResult.IsFailure)
        {
            return Result<AdhkarDto>.Failure(updateResult.Error);
        }

        var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

        return saveResult.IsSuccess
            ? Result<AdhkarDto>.Success(adhkar.ToDto())
            : Result<AdhkarDto>.Failure(saveResult.Error);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var adhkar = await adhkarRepository.FindAsync(AdhkarId.From(id), cancellationToken);
        if (adhkar is null)
        {
            return Result.Failure(AdhkarErrors.NotFound);
        }

        adhkarRepository.Remove(adhkar);
        return await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
