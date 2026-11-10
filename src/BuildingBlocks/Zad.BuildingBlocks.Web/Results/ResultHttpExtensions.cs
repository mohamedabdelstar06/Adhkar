using Microsoft.AspNetCore.Http;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Web.Results;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();

    public static IResult ToCreatedResult<T>(this Result<T> result, string uri) =>
        result.IsSuccess ? TypedResults.Created(uri, result.Value) : result.Error.ToProblem();

    public static IResult ToProblem(this Error error)
    {
        var extensions = new Dictionary<string, object?>
        {
            ["code"] = error.Code
        };

        if (error.Details is { Count: > 0 })
        {
            extensions["details"] = error.Details;
        }

        return TypedResults.Problem(
            statusCode: (int)error.Type,
            title: error.Message,
            type: $"https://errors.zad.example/{error.Code}",
            extensions: extensions);
    }
}
