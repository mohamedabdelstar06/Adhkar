namespace Zad.BuildingBlocks.Contracts.Results;

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public IReadOnlyDictionary<string, object?>? Details { get; init; }

    public static Error Validation(string code, string message, IReadOnlyDictionary<string, object?>? details = null) =>
        new(code, message, ErrorType.Validation) { Details = details };

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public static Error Unprocessable(string code, string message) =>
        new(code, message, ErrorType.Unprocessable);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);
}
