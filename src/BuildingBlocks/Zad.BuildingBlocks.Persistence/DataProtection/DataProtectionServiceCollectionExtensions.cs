using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Persistence.Platform;
using Zad.BuildingBlocks.Persistence.Registration;

namespace Zad.BuildingBlocks.Persistence.DataProtection;

public static class DataProtectionServiceCollectionExtensions
{
    public const string ApplicationName = "Zad";
    public const string TenantDatabasesPurpose = "Zad.TenantDatabases";

    public static IServiceCollection AddZadDataProtection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var catalogConnectionString = configuration.GetConnectionString(PlatformConnectionStrings.Catalog)
            ?? throw new InvalidOperationException("ConnectionStrings:Catalog is required");

        services.AddModuleDbContext<DataProtectionKeysDbContext>(catalogConnectionString);

        services.AddDataProtection()
            .SetApplicationName(ApplicationName)
            .PersistKeysToDbContext<DataProtectionKeysDbContext>();

        return services;
    }
}

