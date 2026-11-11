using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Results;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Domain.Tenants;

namespace Zad.Modules.Tenancy.Api.Tenants;

internal sealed class GetCurrentTenantEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapTenancyGroup()
            .WithTags("Tenancy")
            .MapGet("/current", HandleAsync)
            .WithName("GetCurrentTenant");
    }

    private static async Task<IResult> HandleAsync(
        CurrentTenantReader reader,
        CancellationToken cancellationToken)
    {
        var tenant = await reader.GetAsync(cancellationToken);
        return tenant is null ? TenantErrors.NotFound.ToProblem() : TypedResults.Ok(tenant);
    }
}
