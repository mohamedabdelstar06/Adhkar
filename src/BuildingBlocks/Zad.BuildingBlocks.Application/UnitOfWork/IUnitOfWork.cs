using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Application.UnitOfWork;

public interface IUnitOfWork
{
    public Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);

    public Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> work,
        CancellationToken cancellationToken = default);

    public Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> work,
        CancellationToken cancellationToken = default);
}
