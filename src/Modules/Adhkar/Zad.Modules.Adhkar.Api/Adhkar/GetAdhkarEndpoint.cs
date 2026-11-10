using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using Zad.Modules.Adhkar.Domain.Adhkar;

namespace Zad.Modules.Adhkar.Api.Adhkar;

internal sealed class GetAdhkarEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapAdhkarGroup()
            .MapGet("/{id:guid}", HandleAsync)
            .WithName("GetAdhkar");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        IAdhkarReader reader,
        CancellationToken cancellationToken)
    {
        var item = await reader.GetByIdAsync(id, cancellationToken);
        return item is null ? AdhkarErrors.NotFound.ToProblem() : TypedResults.Ok(item);
    }
}
