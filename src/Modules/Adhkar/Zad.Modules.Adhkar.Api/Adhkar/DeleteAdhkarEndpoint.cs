using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Adhkar.Application.Adhkar;

namespace Zad.Modules.Adhkar.Api.Adhkar;

internal sealed class DeleteAdhkarEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapAdhkarGroup()
            .MapDelete("/{id:guid}", HandleAsync)
            .WithName("DeleteAdhkar");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        AdhkarManagementService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.ToHttpResult();
    }
}
