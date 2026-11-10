using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Tenancy.Application.Tenants;

namespace Zad.Modules.Tenancy.Api.Platform;

internal sealed class ActivatePlatformTenantEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPlatformTenancyGroup()
            .MapPost("/tenants/{id:guid}/activate", HandleAsync)
            .WithName("ActivatePlatformTenant");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        TenantProvisioningService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ActivateAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToHttpResult();
    }
}

