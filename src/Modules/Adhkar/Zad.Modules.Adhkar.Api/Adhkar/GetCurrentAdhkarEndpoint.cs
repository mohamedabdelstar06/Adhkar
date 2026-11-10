using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using Zad.Modules.Adhkar.Domain.Adhkar;

namespace Zad.Modules.Adhkar.Api.Adhkar;

internal sealed class GetCurrentAdhkarEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapAdhkarGroup()
            .MapGet("/current", HandleAsync)
            .WithName("GetCurrentAdhkar");
    }

    private static async Task<IResult> HandleAsync(
        IAdhkarReader reader,
        CancellationToken cancellationToken)
    {
        var current = await reader.GetCurrentAsync(cancellationToken);
        return current is null ? AdhkarErrors.NotFound.ToProblem() : TypedResults.Ok(current);
    }
}
