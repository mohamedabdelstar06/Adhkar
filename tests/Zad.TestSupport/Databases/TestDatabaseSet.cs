extern alias MigrationRunner;

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Identity;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Domain.Identifiers;
using Zad.BuildingBlocks.Persistence.DataProtection;
using Zad.BuildingBlocks.Persistence.Tenancy;
using Zad.Modules.Tenancy.Application.Databases;
using Zad.Modules.Tenancy.Application.Tenants;
using Zad.Modules.Tenancy.Domain.Databases;
using Zad.Modules.Tenancy.Infrastructure;
using Zad.TestSupport.Tenancy;
using RunnerCli = MigrationRunner::Zad.MigrationRunner.Cli;
using RunnerDiscovery = MigrationRunner::Zad.MigrationRunner.Discovery;
using RunnerExecution = MigrationRunner::Zad.MigrationRunner.Execution;
using RunnerJournal = MigrationRunner::Zad.MigrationRunner.Journal;
using RunnerTargets = MigrationRunner::Zad.MigrationRunner.Targets;

namespace Zad.TestSupport.Databases;

public sealed class TestDatabaseSet : IAsyncDisposable
{
    private const string SharedDatabaseLogicalName = "Shared01";

    private readonly SqlServerFixture _fixture;
    private readonly DatabaseResetter _resetter;

    private TestDatabaseSet(
        SqlServerFixture fixture,
        string catalogDatabaseName,
        string sharedDatabaseName,
        string jobsDatabaseName,
        string catalogConnectionString,
        string sharedConnectionString,
        string jobsConnectionString,
        DatabaseResetter resetter)
    {
        _fixture = fixture;
        CatalogDatabaseName = catalogDatabaseName;
        SharedDatabaseName = sharedDatabaseName;
        JobsDatabaseName = jobsDatabaseName;
        CatalogConnectionString = catalogConnectionString;
        SharedConnectionString = sharedConnectionString;
        JobsConnectionString = jobsConnectionString;
        _resetter = resetter;
    }

    public string CatalogDatabaseName { get; }
    public string SharedDatabaseName { get; }
    public string JobsDatabaseName { get; }
    public string CatalogConnectionString { get; }
    public string SharedConnectionString { get; }
    public string JobsConnectionString { get; }

    public static async Task<TestDatabaseSet> CreateAsync(
        SqlServerFixture fixture,
        CancellationToken cancellationToken = default)
    {
        var suffix = GuidV7.New().ToString("N")[..12];
        var catalogDbName = $"ZadCatalog_{suffix}";
        var sharedDbName = $"ZadShared01_{suffix}";
        var jobsDbName = $"ZadJobs_{suffix}";

        var catalogConn = fixture.BuildDatabaseConnectionString(catalogDbName);
        var sharedConn = fixture.BuildDatabaseConnectionString(sharedDbName);
        var jobsConn = fixture.BuildDatabaseConnectionString(jobsDbName);

        try
        {
            await EnsureDatabaseCreatedAsync(fixture.MasterConnectionString, catalogDbName, cancellationToken);
            await EnsureDatabaseCreatedAsync(fixture.MasterConnectionString, sharedDbName, cancellationToken);
            await EnsureDatabaseCreatedAsync(fixture.MasterConnectionString, jobsDbName, cancellationToken);

            await RunMigrationsAsync(catalogConn, sharedConn, jobsConn, cancellationToken);
            await RegisterSharedDatabaseAsync(catalogConn, sharedConn, cancellationToken);

            var resetter = await DatabaseResetter.CreateAsync(catalogConn, sharedConn, cancellationToken);

            return new TestDatabaseSet(
                fixture,
                catalogDbName,
                sharedDbName,
                jobsDbName,
                catalogConn,
                sharedConn,
                jobsConn,
                resetter);
        }
        catch
        {
            SqlConnection.ClearAllPools();
            await DropDatabaseIfExistsAsync(fixture.MasterConnectionString, catalogDbName);
            await DropDatabaseIfExistsAsync(fixture.MasterConnectionString, sharedDbName);
            await DropDatabaseIfExistsAsync(fixture.MasterConnectionString, jobsDbName);
            throw;
        }
    }

    public async Task<ProvisionedTenants> ResetAndProvisionAsync(
        IServiceProvider rootServices,
        CancellationToken cancellationToken = default)
    {
        await _resetter.ResetAsync(cancellationToken);

        await using var scope = rootServices.CreateAsyncScope();
        var provisioning = scope.ServiceProvider.GetRequiredService<TenantProvisioningService>();

        var registerDb = await provisioning.RegisterDatabaseAsync(
            new RegisterDatabaseRequest(SharedDatabaseLogicalName, DatabaseKind.Shared, SharedConnectionString, SharedConnectionString),
            cancellationToken);

        if (registerDb.IsFailure)
        {
            throw new InvalidOperationException($"Failed to register shared test database: {registerDb.Error.Code} - {registerDb.Error.Message}");
        }

        var alphaResult = await provisioning.RegisterAsync(
            new RegisterTenantRequest("alpha", "alpha.localhost", registerDb.Value, "ar", "UTC"),
            cancellationToken);

        if (alphaResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to provision alpha tenant: {alphaResult.Error.Code} - {alphaResult.Error.Message}");
        }

        var bravoResult = await provisioning.RegisterAsync(
            new RegisterTenantRequest("bravo", "bravo.localhost", registerDb.Value, "en", "UTC"),
            cancellationToken);

        if (bravoResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to provision bravo tenant: {bravoResult.Error.Code} - {bravoResult.Error.Message}");
        }

        return new ProvisionedTenants(
            new TenantHandle(alphaResult.Value, "alpha", "alpha.localhost"),
            new TenantHandle(bravoResult.Value, "bravo", "bravo.localhost"));
    }

    public async ValueTask DisposeAsync()
    {
        SqlConnection.ClearAllPools();

        await DropDatabaseIfExistsAsync(_fixture.MasterConnectionString, CatalogDatabaseName);
        await DropDatabaseIfExistsAsync(_fixture.MasterConnectionString, SharedDatabaseName);
        await DropDatabaseIfExistsAsync(_fixture.MasterConnectionString, JobsDatabaseName);
    }

    private static async Task EnsureDatabaseCreatedAsync(
        string masterConnectionString,
        string databaseName,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(
            """
            IF DB_ID(@name) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) =
                    N'CREATE DATABASE ' + QUOTENAME(@name) +
                    N'; ALTER DATABASE ' + QUOTENAME(@name) + N' SET READ_COMMITTED_SNAPSHOT ON;';
                EXEC(@sql);
            END
            """,
            connection);
        command.CommandTimeout = 120;
        command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = databaseName });
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task DropDatabaseIfExistsAsync(string masterConnectionString, string databaseName)
    {
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            """
            IF DB_ID(@name) IS NOT NULL
            BEGIN
                DECLARE @sql nvarchar(max) =
                    N'ALTER DATABASE ' + QUOTENAME(@name) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@name) + N';';
                EXEC(@sql);
            END
            """,
            connection);
        command.CommandTimeout = 120;
        command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = databaseName });
        await command.ExecuteNonQueryAsync();
    }

    private static async Task RunMigrationsAsync(
        string catalogConnectionString,
        string sharedConnectionString,
        string jobsConnectionString,
        CancellationToken cancellationToken)
    {
        const string catalogVar = "ZAD_MIGRATIONS_CATALOG_CONNECTION_STRING";
        const string sharedVar = "ZAD_MIGRATIONS_SHARED_CONNECTION_STRING";
        const string jobsVar = "ZAD_MIGRATIONS_JOBS_CONNECTION_STRING";

        var previousCatalog = Environment.GetEnvironmentVariable(catalogVar);
        var previousShared = Environment.GetEnvironmentVariable(sharedVar);
        var previousJobs = Environment.GetEnvironmentVariable(jobsVar);

        try
        {
            Environment.SetEnvironmentVariable(catalogVar, catalogConnectionString);
            Environment.SetEnvironmentVariable(sharedVar, sharedConnectionString);
            Environment.SetEnvironmentVariable(jobsVar, jobsConnectionString);

            var migrationConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    [catalogVar] = catalogConnectionString,
                    [sharedVar] = sharedConnectionString,
                    [jobsVar] = jobsConnectionString
                })
                .Build();

            var log = new RunnerExecution.RunnerLog();
            var journal = new RunnerJournal.MigrationJournal();
            var executor = new RunnerExecution.MigrationExecutor(
                new RunnerTargets.MigrationTargetSource(migrationConfig),
                new RunnerDiscovery.ModuleContextDiscovery(),
                journal,
                new RunnerExecution.DatabaseBootstrap(log),
                new RunnerExecution.HangfireSchemaInstaller(journal, log),
                new RunnerExecution.ScriptWriter(log),
                log);

            var options = RunnerCli.RunnerOptions.Parse(["--target", "ci", "--databases", "all"]);
            var exitCode = await executor.RunAsync(options, cancellationToken);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"MigrationExecutor failed with exit code {exitCode}.");
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable(catalogVar, previousCatalog);
            Environment.SetEnvironmentVariable(sharedVar, previousShared);
            Environment.SetEnvironmentVariable(jobsVar, previousJobs);
        }
    }

    private static async Task RegisterSharedDatabaseAsync(
        string catalogConnectionString,
        string sharedConnectionString,
        CancellationToken cancellationToken)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:Catalog"] = catalogConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ITenantContext>(UnresolvedTenantContext.Instance);
        services.AddScoped<ICurrentUser, PreBootCurrentUser>();
        services.AddZadDataProtection(configuration);
        new TenancyModuleInstaller().Install(services, configuration);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var provisioning = scope.ServiceProvider.GetRequiredService<TenantProvisioningService>();
        var result = await provisioning.RegisterDatabaseAsync(
            new RegisterDatabaseRequest(SharedDatabaseLogicalName, DatabaseKind.Shared, sharedConnectionString, sharedConnectionString),
            cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Failed to pre-register shared database: {result.Error.Code} - {result.Error.Message}");
        }
    }

    private sealed class PreBootCurrentUser : ICurrentUser
    {
        public Guid? UserId => null;
        public ActorType ActorType => ActorType.System;
        public bool IsAuthenticated => false;
    }
}
