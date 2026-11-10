using Hangfire;
using Hangfire.Dashboard;
using System.Text.Json.Serialization;
using Zad.BuildingBlocks.Application.Composition;
using Zad.BuildingBlocks.Application.Identity;
using Zad.BuildingBlocks.Application.Localization;
using Zad.BuildingBlocks.Application.Migrations;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Jobs.Hosting;
using Zad.BuildingBlocks.Messaging.Registration;
using Zad.BuildingBlocks.Persistence.DataProtection;
using Zad.BuildingBlocks.Persistence.Migrations;
using Zad.BuildingBlocks.Persistence.Tenancy;
using Serilog;
using Zad.BuildingBlocks.Web.Endpoints;
using Zad.BuildingBlocks.Web.Errors;
using Zad.BuildingBlocks.Web.Identity;
using Zad.BuildingBlocks.Web.Localization;
using Zad.BuildingBlocks.Web.Observability;
using Zad.BuildingBlocks.Web.Tenancy;
using Zad.Modules.Adhkar.Api;
using Zad.Modules.Adhkar.Infrastructure;
using Zad.Modules.Catalog.Contracts.Events;
using Zad.Modules.Catalog.Api;
using Zad.Modules.Catalog.Infrastructure;
using Zad.Modules.Orders.Api;
using Zad.Modules.Orders.Contracts.Events;
using Zad.Modules.Orders.Infrastructure;
using Zad.Modules.Tenancy.Api;
using Zad.Modules.Tenancy.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        formatProvider: System.Globalization.CultureInfo.InvariantCulture));

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
}

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ITenantContextSetter>(provider => provider.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();
builder.Services.AddScoped<CorrelationContext>();
builder.Services.AddScoped<ICorrelationContext>(provider => provider.GetRequiredService<CorrelationContext>());
builder.Services.AddScoped<ICorrelationContextSetter>(provider => provider.GetRequiredService<CorrelationContext>());
builder.Services.AddScoped<CultureContext>();
builder.Services.AddScoped<ICultureContext>(provider => provider.GetRequiredService<CultureContext>());
builder.Services.AddScoped<ICultureContextSetter>(provider => provider.GetRequiredService<CultureContext>());

builder.Services.AddOpenApi();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddZadDataProtection(builder.Configuration);

builder.Services.AddMessaging(
    typeof(OrderPlacedIntegrationEvent).Assembly,
    typeof(ProductCreatedIntegrationEvent).Assembly);

builder.Services.AddModules(
    builder.Configuration,
    typeof(TenancyModuleInstaller).Assembly,
    typeof(CatalogModuleInstaller).Assembly,
    typeof(OrdersModuleInstaller).Assembly,
    typeof(AdhkarModuleInstaller).Assembly);

builder.Services.AddZadJobs(builder.Configuration);

builder.Services.AddSingleton<PendingMigrationsGuard>();
builder.Services.AddSingleton<ITenantDatabaseReadiness>(provider => provider.GetRequiredService<PendingMigrationsGuard>());
builder.Services.AddHostedService<Zad.Host.Tenancy.TenancyRegistryWarmup>();

var app = builder.Build();

var registry = app.Services.GetRequiredService<Zad.Modules.Tenancy.Infrastructure.Databases.TenancyRegistry>();
await registry.RefreshAsync(app.Lifetime.ApplicationStopping);

await app.Services.GetRequiredService<PendingMigrationsGuard>()
    .EnsureStartupDatabasesCurrentAsync(app.Lifetime.ApplicationStopping);

app.UseCorrelationId();
app.UseZadExceptionHandling();
app.UseCors();
app.UseTenantResolution();
app.UseCultureResolution();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapHangfireDashboard("/ops/jobs", new DashboardOptions
    {
        Authorization = [new LocalRequestsOnlyAuthorizationFilter()],
        DisplayStorageConnectionString = false
    });
}

app.MapModuleEndpoints(
    typeof(TenancyApiMarker).Assembly,
    typeof(CatalogApiMarker).Assembly,
    typeof(OrdersApiMarker).Assembly,
    typeof(AdhkarApiMarker).Assembly);

app.MapFallback("/api/{**rest}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

await app.RunAsync();

public partial class Program;
