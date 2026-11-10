namespace Zad.Modules.Adhkar.Domain.Adhkar;

public interface IAdhkarRepository
{
    public Task<Adhkar?> FindAsync(AdhkarId id, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<Adhkar>> ListActiveOrderedAsync(CancellationToken cancellationToken = default);

    public Task<bool> DisplayOrderTakenAsync(int displayOrder, AdhkarId? excluded, CancellationToken cancellationToken = default);

    public void Add(Adhkar adhkar);

    public void Remove(Adhkar adhkar);
}
