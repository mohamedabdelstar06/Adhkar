using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Tenancy.Application.Tenants;

namespace Zad.Modules.Tenancy.Api.Platform;

internal sealed class DeletePlatformTenantEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPlatformTenancyGroup()
            .MapDelete("/tenants/{id:guid}", HandleAsync)
            .WithName("DeletePlatformTenant");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        TenantProvisioningService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToHttpResult();
    }
}

