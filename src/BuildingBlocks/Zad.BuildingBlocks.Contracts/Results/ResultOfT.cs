namespace Zad.BuildingBlocks.Contracts.Results;

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value of a failed result: {_error?.Code}");

    public Error Error => IsSuccess
        ? throw new InvalidOperationException("Cannot access Error of a successful result")
        : _error!;

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(Error error) => new(false, default, error ?? throw new ArgumentNullException(nameof(error)));
}
