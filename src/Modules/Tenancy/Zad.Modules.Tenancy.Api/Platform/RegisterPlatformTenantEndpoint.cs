using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.BuildingBlocks.Web.Validation;
using Zad.Modules.Tenancy.Application.Tenants;

namespace Zad.Modules.Tenancy.Api.Platform;

internal sealed class RegisterPlatformTenantEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPlatformTenancyGroup()
            .MapPost("/tenants", HandleAsync)
            .WithValidation<ProvisionTenantRequest>()
            .WithName("RegisterPlatformTenant");
    }

    private static async Task<IResult> HandleAsync(
        ProvisionTenantRequest request,
        TenantProvisioningService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RegisterAsync(
            new RegisterTenantRequest(
                request.Slug,
                request.Host,
                DatabaseId: null,
                request.DefaultLanguage,
                request.TimeZone),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Created($"/api/platform/tenants/{result.Value}", new { tenantId = result.Value })
            : result.ToHttpResult();
    }
}
