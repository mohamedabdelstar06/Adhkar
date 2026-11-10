using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Application.UnitOfWork;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Persistence.UnitOfWork;

public abstract class ModuleUnitOfWork<TContext>(TContext context) : IUnitOfWork where TContext : DbContext
{
    protected abstract Error ConcurrencyConflict { get; }

    protected virtual IReadOnlyDictionary<string, Error> UniqueIndexErrors { get; } =
        new Dictionary<string, Error>(StringComparer.OrdinalIgnoreCase);

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(ConcurrencyConflict);
        }
        catch (DbUpdateException exception) when (SqlFailure.Classify(exception, UniqueIndexErrors) is { } error)
        {
            return Result.Failure(error);
        }
    }

    public async Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> work,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async token =>
            {
                context.ChangeTracker.Clear();

                await using var transaction = await context.Database.BeginTransactionAsync(token);
                var result = await work(token);

                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(token);
                }
                else
                {
                    await transaction.RollbackAsync(token);
                }

                return result;
            },
            cancellationToken);
    }

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> work,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async token =>
            {
                context.ChangeTracker.Clear();

                await using var transaction = await context.Database.BeginTransactionAsync(token);
                var result = await work(token);

                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(token);
                }
                else
                {
                    await transaction.RollbackAsync(token);
                }

                return result;
            },
            cancellationToken);
    }
}
