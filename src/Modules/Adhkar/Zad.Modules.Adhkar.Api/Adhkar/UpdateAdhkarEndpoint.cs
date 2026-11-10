using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.BuildingBlocks.Web.Validation;
using Zad.Modules.Adhkar.Application.Adhkar;

namespace Zad.Modules.Adhkar.Api.Adhkar;

internal sealed class UpdateAdhkarEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapAdhkarGroup()
            .MapPut("/{id:guid}", HandleAsync)
            .WithValidation<UpdateAdhkarRequest>()
            .WithName("UpdateAdhkar");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdateAdhkarRequest request,
        AdhkarManagementService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return result.ToHttpResult();
    }
}
