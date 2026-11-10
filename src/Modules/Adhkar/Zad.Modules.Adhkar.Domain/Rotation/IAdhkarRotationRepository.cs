namespace Zad.Modules.Adhkar.Domain.Rotation;

public interface IAdhkarRotationRepository
{
    public Task<AdhkarRotation?> GetAsync(CancellationToken cancellationToken = default);

    public void Add(AdhkarRotation rotation);
}
