
namespace Zad.Modules.Tenancy.Application.Tenants;

public sealed class TenantAdminReader(ITenantReadRepository readRepository)
{
    public Task<IReadOnlyList<TenantRegistrationDto>> ListAsync(CancellationToken cancellationToken = default) =>
        readRepository.ListAllAsync(cancellationToken);
}
