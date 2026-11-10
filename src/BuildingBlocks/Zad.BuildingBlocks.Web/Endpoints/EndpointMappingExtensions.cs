using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Zad.BuildingBlocks.Web.Endpoints;

public static class EndpointMappingExtensions
{
    public static WebApplication MapModuleEndpoints(this WebApplication app, params Assembly[] apiAssemblies)
    {
        foreach (var assembly in apiAssemblies)
        {
            var endpointTypes = assembly.GetTypes()
                .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToList();

            foreach (var type in endpointTypes)
            {
                var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
                endpoint.Map(app);
            }
        }

        return app;
    }
}

