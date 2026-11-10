using Zad.Modules.Adhkar.Contracts.Adhkar;

namespace Zad.Modules.Adhkar.Application.Adhkar;

public interface IAdhkarReadRepository
{
    public Task<IReadOnlyList<AdhkarDto>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default);

    public Task<AdhkarDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<AdhkarDto?> GetFirstActiveAsync(CancellationToken cancellationToken = default);

    public Task<Guid?> GetRotationCurrentIdAsync(CancellationToken cancellationToken = default);
}
