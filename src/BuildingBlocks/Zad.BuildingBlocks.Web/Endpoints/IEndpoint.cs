using Microsoft.AspNetCore.Routing;

namespace Zad.BuildingBlocks.Web.Endpoints;

public interface IEndpoint
{
    public void Map(IEndpointRouteBuilder app);
}

