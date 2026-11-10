using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.Modules.Adhkar.Contracts.Adhkar;

namespace Zad.Modules.Adhkar.Api.Adhkar;

internal sealed class ListAdhkarEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapAdhkarGroup()
            .MapGet(string.Empty, HandleAsync)
            .WithName("ListAdhkar");
    }

    private static async Task<IResult> HandleAsync(
        bool? includeInactive,
        IAdhkarReader reader,
        CancellationToken cancellationToken)
    {
        var items = await reader.ListAsync(includeInactive.GetValueOrDefault(false), cancellationToken);
        return TypedResults.Ok(items);
    }
}
