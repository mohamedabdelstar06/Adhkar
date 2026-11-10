using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.BuildingBlocks.Web.Validation;
using Zad.Modules.Tenancy.Application.Tenants;

namespace Zad.Modules.Tenancy.Api.Platform;

internal sealed class UpdatePlatformTenantEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPlatformTenancyGroup()
            .MapPut("/tenants/{id:guid}", HandleAsync)
            .WithValidation<UpdatePlatformTenantRequest>()
            .WithName("UpdatePlatformTenant");
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        UpdatePlatformTenantRequest request,
        TenantProvisioningService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToHttpResult();
    }
}

