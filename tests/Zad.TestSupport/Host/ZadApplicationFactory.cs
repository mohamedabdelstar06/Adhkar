using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zad.BuildingBlocks.Messaging.Processing;
using Zad.TestSupport.Databases;
using Zad.TestSupport.Tenancy;

namespace Zad.TestSupport.Host;

public sealed class ZadApplicationFactory(TestDatabaseSet databases) : WebApplicationFactory<Program>
{
    public HttpClient CreateClientForTenant(TenantHandle tenant)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Host = tenant.Host;
        return client;
    }

    public HttpClient CreatePlatformClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Host = "admin.localhost";
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Catalog", databases.CatalogConnectionString);
        builder.UseSetting("ConnectionStrings:Jobs", databases.JobsConnectionString);
        builder.UseSetting("Tenancy:RootDomain", "localhost");
        builder.UseSetting("AllowedHosts", "*");
        builder.UseSetting("Jobs:ServerEnabled", "false");
        builder.UseSetting("Messaging:ProcessorsEnabled", "false");
        builder.UseSetting("Adhkar:RotationCron", "0 * * * * *");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:Catalog"] = databases.CatalogConnectionString,
                ["ConnectionStrings:Jobs"] = databases.JobsConnectionString,
                ["Tenancy:RootDomain"] = "localhost",
                ["AllowedHosts"] = "*",
                ["Jobs:ServerEnabled"] = "false",
                ["Messaging:ProcessorsEnabled"] = "false",
                ["Adhkar:RotationCron"] = "0 * * * * *"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var backgroundProcessors = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService) && IsMessagingProcessor(descriptor.ImplementationType))
                .ToList();

            foreach (var descriptor in backgroundProcessors)
            {
                services.Remove(descriptor);
            }
        });
    }

    private static bool IsMessagingProcessor(Type? implementationType)
    {
        if (implementationType is null || !implementationType.IsGenericType)
        {
            return false;
        }

        var definition = implementationType.GetGenericTypeDefinition();
        return definition == typeof(OutboxProcessor<>) || definition == typeof(InboxProcessor<>);
    }
}
