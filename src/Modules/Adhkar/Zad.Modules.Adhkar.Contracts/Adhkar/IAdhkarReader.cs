namespace Zad.Modules.Adhkar.Contracts.Adhkar;

public interface IAdhkarReader
{
    public Task<IReadOnlyList<AdhkarDto>> ListAsync(bool includeInactive, CancellationToken cancellationToken = default);

    public Task<AdhkarDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<AdhkarDto?> GetCurrentAsync(CancellationToken cancellationToken = default);
}
