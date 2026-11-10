using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.Modules.Tenancy.Application.Tenants;

namespace Zad.Modules.Tenancy.Api.Platform;

internal sealed class ListPlatformTenantsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPlatformTenancyGroup()
            .MapGet("/tenants", HandleAsync)
            .WithName("ListPlatformTenants");
    }

    private static async Task<IResult> HandleAsync(
        TenantAdminReader reader,
        CancellationToken cancellationToken)
    {
        var tenants = await reader.ListAsync(cancellationToken);

        return TypedResults.Ok(tenants);
    }
}
