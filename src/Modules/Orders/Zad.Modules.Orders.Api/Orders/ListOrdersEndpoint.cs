using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.Modules.Orders.Contracts.Orders;

namespace Zad.Modules.Orders.Api.Orders;

internal sealed class ListOrdersEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapOrdersGroup()
            .MapGet(string.Empty, HandleAsync)
            .WithName("ListOrders");
    }

    private static async Task<IResult> HandleAsync(
        int? page,
        int? pageSize,
        IOrderReader reader,
        CancellationToken cancellationToken)
    {
        var result = await reader.ListAsync(
            page.GetValueOrDefault(1),
            pageSize.GetValueOrDefault(10),
            cancellationToken);

        return TypedResults.Ok(result);
    }
}
