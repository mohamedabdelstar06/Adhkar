using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.MigrationRunner.Execution;
using Zad.Modules.Adhkar.Application.Adhkar;
using Zad.Modules.Adhkar.Contracts.Adhkar;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Application.Security;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Contracts;
using Zad.Modules.Tenancy.Domain.Databases;
using Zad.Modules.Tenancy.Infrastructure.Databases;

namespace Zad.MigrationRunner.Seeding;

public sealed class DevEnvironmentSeeder(IConfiguration configuration, RunnerLog log)
{
    private const string SharedDatabaseName = "ZadShared01";
    private const string DemoTenantSlug = "demo";
    private const string DemoTenantHost = "demo.localhost";

    private static readonly (string Text, string Translation, string Reference, string Category, int Order)[] SeedAdhkar =
    [
        ("Ø³ÙØ¨Ù’Ø­ÙŽØ§Ù†ÙŽ Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù", "Glory be to Allah", "Muslim 2694", "tasbih", 1),
        ("Ø§Ù„Ù’Ø­ÙŽÙ…Ù’Ø¯Ù Ù„ÙÙ„ÙŽÙ‘Ù‡Ù", "All praise is due to Allah", "Muslim 2694", "tahmid", 2),
        ("Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù Ø£ÙŽÙƒÙ’Ø¨ÙŽØ±Ù", "Allah is the Greatest", "Muslim 2694", "takbir", 3),
        ("Ù„ÙŽØ§ Ø¥ÙÙ„ÙŽÙ‡ÙŽ Ø¥ÙÙ„ÙŽÙ‘Ø§ Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù", "There is no god but Allah", "Muslim 2694", "tahlil", 4),
        ("Ù„ÙŽØ§ Ø­ÙŽÙˆÙ’Ù„ÙŽ ÙˆÙŽÙ„ÙŽØ§ Ù‚ÙÙˆÙŽÙ‘Ø©ÙŽ Ø¥ÙÙ„ÙŽÙ‘Ø§ Ø¨ÙØ§Ù„Ù„ÙŽÙ‘Ù‡Ù", "There is no power and no strength except with Allah", "Bukhari 6384", "hawqala", 5),
        ("Ø§Ù„Ù„ÙŽÙ‘Ù‡ÙÙ…ÙŽÙ‘ ØµÙŽÙ„ÙÙ‘ Ø¹ÙŽÙ„ÙŽÙ‰ Ù…ÙØ­ÙŽÙ…ÙŽÙ‘Ø¯Ù", "O Allah, send blessings upon Muhammad", "Bukhari 3370", "salawat", 6),
        ("Ø³ÙØ¨Ù’Ø­ÙŽØ§Ù†ÙŽ Ø§Ù„Ù„ÙŽÙ‘Ù‡Ù ÙˆÙŽØ¨ÙØ­ÙŽÙ…Ù’Ø¯ÙÙ‡Ù", "Glory be to Allah and praise be to Him", "Muslim 2691", "tasbih", 7),
        ("Ø£ÙŽØ³Ù’ØªÙŽØºÙ’ÙÙØ±Ù Ø§Ù„Ù„ÙŽÙ‘Ù‡ÙŽ", "I seek forgiveness from Allah", "Muslim 2702", "tahlil", 8)
    ];

    public async Task<bool> SeedAsync(CancellationToken cancellationToken)
    {
        var sharedConnectionString = configuration.GetConnectionString("Shared");
        if (sharedConnectionString is null)
        {
            log.Warn("ConnectionStrings:Shared is missing; the demo tenant was not seeded");
            return false;
        }

        await using var provider = ModuleServiceProviderFactory.Build(configuration);
        await using var scope = provider.CreateAsyncScope();

        var protector = scope.ServiceProvider.GetRequiredService<IConnectionStringProtector>();
        var provisioning = scope.ServiceProvider.GetRequiredService<TenantProvisioningService>();
        var registry = scope.ServiceProvider.GetRequiredService<TenancyRegistry>();

        var databaseResult = await provisioning.RegisterDatabaseAsync(
            new RegisterDatabaseRequest(
                SharedDatabaseName,
                DatabaseKind.Shared,
                protector.Protect(sharedConnectionString),
                protector.Protect(sharedConnectionString)),
            cancellationToken);

        if (databaseResult.IsFailure)
        {
            log.Error($"Registering the shared database failed: {databaseResult.Error.Message}");
            return false;
        }

        var tenantResult = await provisioning.RegisterAsync(
            new RegisterTenantRequest(DemoTenantSlug, DemoTenantHost, databaseResult.Value, "ar", "UTC"),
            cancellationToken);

        if (tenantResult.IsFailure)
        {
            log.Error($"Registering the demo tenant failed: {tenantResult.Error.Message}");
            return false;
        }

        await registry.RefreshAsync(cancellationToken);

        scope.ServiceProvider.GetRequiredService<ITenantContextSetter>().Set(tenantResult.Value);

        await SeedAdhkarAsync(scope.ServiceProvider, cancellationToken);

        var activateResult = await provisioning.ActivateAsync(tenantResult.Value, cancellationToken);
        if (activateResult.IsFailure &&
            activateResult.Error.Code != TenancyErrorCodes.TenantAlreadyActive)
        {
            log.Error($"Activating the demo tenant failed: {activateResult.Error.Message}");
            return false;
        }

        log.Info($"Demo tenant '{DemoTenantSlug}' is active on {SharedDatabaseName} at {DemoTenantHost}");
        return true;
    }

    private async Task SeedAdhkarAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var reader = services.GetRequiredService<IAdhkarReader>();
        var existing = await reader.ListAsync(includeInactive: true, cancellationToken);

        if (existing.Count > 0)
        {
            return;
        }

        var management = services.GetRequiredService<AdhkarManagementService>();

        foreach (var item in SeedAdhkar)
        {
            var result = await management.CreateAsync(
                new CreateAdhkarRequest(item.Text, item.Translation, item.Reference, item.Category, item.Order, true),
                cancellationToken);

            if (result.IsFailure)
            {
                log.Warn($"Seeding dhikr #{item.Order} failed: {result.Error.Message}");
            }
        }

        log.Info($"Seeded {SeedAdhkar.Length} adhkar for the demo tenant");
    }
}
