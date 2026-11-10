namespace Zad.BuildingBlocks.Contracts.Results;

public sealed class Result
{
    private readonly Error? _error;

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error of a successful result")
        : _error!;

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));
}
