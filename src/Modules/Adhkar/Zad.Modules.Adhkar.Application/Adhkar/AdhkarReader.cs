using Zad.Modules.Adhkar.Contracts.Adhkar;

namespace Zad.Modules.Adhkar.Application.Adhkar;

public sealed class AdhkarReader(IAdhkarReadRepository readRepository) : IAdhkarReader
{
    public Task<IReadOnlyList<AdhkarDto>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
        readRepository.ListAsync(includeInactive, cancellationToken);

    public Task<AdhkarDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        readRepository.GetByIdAsync(id, cancellationToken);

    public async Task<AdhkarDto?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var currentId = await readRepository.GetRotationCurrentIdAsync(cancellationToken);

        if (currentId is { } id)
        {
            var current = await readRepository.GetByIdAsync(id, cancellationToken);
            if (current is not null && current.IsActive)
            {
                return current;
            }
        }

        return await readRepository.GetFirstActiveAsync(cancellationToken);
    }
}
