namespace Zad.BuildingBlocks.Contracts.Results;

public enum ErrorType
{
    Validation = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    Conflict = 409,
    Unprocessable = 422,
    TooManyRequests = 429,
    Failure = 500
}
