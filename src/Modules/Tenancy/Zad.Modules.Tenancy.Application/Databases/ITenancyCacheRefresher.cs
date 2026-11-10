namespace Zad.Modules.Tenancy.Application.Databases;

public interface ITenancyCacheRefresher
{
    public Task RefreshAsync(CancellationToken cancellationToken = default);
}
