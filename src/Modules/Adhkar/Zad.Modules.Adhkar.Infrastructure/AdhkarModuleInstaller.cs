using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Composition;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Jobs.Scheduling;
using Zad.BuildingBlocks.Messaging.Registration;
using Zad.BuildingBlocks.Persistence.Connections;
using Zad.BuildingBlocks.Persistence.Registration;
using Zad.Modules.Adhkar.Application.Adhkar;
using Zad.Modules.Adhkar.Application.Rotation;
using Zad.Modules.Adhkar.Application;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using Zad.Modules.Adhkar.Infrastructure.Adhkar;
using Zad.Modules.Adhkar.Infrastructure.Persistence;
using Zad.Modules.Adhkar.Infrastructure.Persistence.ReadRepositories;
using Zad.Modules.Adhkar.Infrastructure.Persistence.Repositories;

namespace Zad.Modules.Adhkar.Infrastructure;

public sealed class AdhkarModuleInstaller : IModuleInstaller
{
    public const string RotationJobId = "adhkar.rotation";
    public const string DefaultRotationCron = "0 * * * * *";

    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<AdhkarDbContext>(provider =>
            provider.GetRequiredService<ITenantConnectionResolver>()
                .ResolveApplicationConnectionString(provider.GetRequiredService<ITenantContext>().TenantId));

        services.AddScoped<Domain.Adhkar.IAdhkarRepository, AdhkarRepository>();
        services.AddScoped<Domain.Rotation.IAdhkarRotationRepository, AdhkarRotationRepository>();
        services.AddScoped<IAdhkarReadRepository, AdhkarReadRepository>();
        services.AddScoped<IAdhkarUnitOfWork, AdhkarUnitOfWork>();

        services.AddScoped<AdhkarManagementService>();
        services.AddScoped<AdhkarRotationService>();
        services.AddScoped<AdhkarReader>();
        services.AddScoped<IAdhkarReader>(provider => provider.GetRequiredService<AdhkarReader>());

        services.AddModuleMessaging<AdhkarDbContext, IAdhkarUnitOfWork>("Adhkar", AdhkarDbContext.SchemaName);

        var rotationCron = configuration["Adhkar:RotationCron"];
        services.AddTenantSweep<AdhkarRotationJob>(
            RotationJobId,
            string.IsNullOrWhiteSpace(rotationCron) ? DefaultRotationCron : rotationCron);

        services.AddValidatorsFromAssembly(typeof(CreateAdhkarRequest).Assembly, includeInternalTypes: true);
    }
}
