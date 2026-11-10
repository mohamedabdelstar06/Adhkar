using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.Modules.Tenancy.Contracts;

namespace Zad.Modules.Tenancy.Api.Tenants;

internal sealed class GetCurrentTenantEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/tenancy")
            .WithTags("Tenancy")
            .MapGet("/current", HandleAsync)
            .WithName("GetCurrentTenant");
    }

    private static async Task<IResult> HandleAsync(
        ITenantContext tenantContext,
        ITenantDirectory directory,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.IsResolved)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "The request did not resolve a tenant",
                extensions: new Dictionary<string, object?> { ["code"] = TenancyErrorCodes.TenantNotFound });
        }

        var tenant = await directory.GetAsync(tenantContext.TenantId, cancellationToken);

        return tenant is null
            ? TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "The requested tenant does not exist",
                extensions: new Dictionary<string, object?> { ["code"] = TenancyErrorCodes.TenantNotFound })
            : TypedResults.Ok(tenant);
    }
}
