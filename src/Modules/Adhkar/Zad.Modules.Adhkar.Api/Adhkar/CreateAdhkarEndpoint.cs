using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.BuildingBlocks.Web.Validation;
using Zad.Modules.Adhkar.Application.Adhkar;

namespace Zad.Modules.Adhkar.Api.Adhkar;

internal sealed class CreateAdhkarEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapAdhkarGroup()
            .MapPost(string.Empty, HandleAsync)
            .WithValidation<CreateAdhkarRequest>()
            .WithName("CreateAdhkar");
    }

    private static async Task<IResult> HandleAsync(
        CreateAdhkarRequest request,
        AdhkarManagementService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? result.ToCreatedResult($"/api/adhkar/{result.Value.Id}")
            : result.ToHttpResult();
    }
}
