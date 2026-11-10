using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;
using Zad.TestSupport.Host;

namespace Zad.TestSupport.Databases;

public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string ExternalConnectionEnvVar = "ZAD_TEST_SQL_CONNECTION_STRING";

    private MsSqlContainer? _container;
    private string _baseConnectionString = string.Empty;
    private TestDatabaseSet? _sharedDatabases;
    private ZadApplicationFactory? _sharedFactory;

    public string MasterConnectionString => BuildDatabaseConnectionString("master");

    public bool UsingExternalServer { get; private set; }

    public async ValueTask InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable(ExternalConnectionEnvVar);
        if (!string.IsNullOrWhiteSpace(external))
        {
            UsingExternalServer = true;
            _baseConnectionString = external;
            return;
        }

        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await _container.StartAsync();
        _baseConnectionString = _container.GetConnectionString();
    }

    public async Task<(TestDatabaseSet Databases, ZadApplicationFactory Factory)> EnsureSharedHostAsync(
        CancellationToken cancellationToken = default)
    {
        if (_sharedDatabases is null || _sharedFactory is null)
        {
            _sharedDatabases = await TestDatabaseSet.CreateAsync(this, cancellationToken);
            _sharedFactory = new ZadApplicationFactory(_sharedDatabases);
            _ = _sharedFactory.Services;
        }

        return (_sharedDatabases, _sharedFactory);
    }

    public string BuildDatabaseConnectionString(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(_baseConnectionString))
        {
            throw new InvalidOperationException("SqlServerFixture has not been initialized yet.");
        }

        var builder = new SqlConnectionStringBuilder(_baseConnectionString)
        {
            InitialCatalog = databaseName,
            MultipleActiveResultSets = true,
            ConnectTimeout = 60
        };

        return builder.ConnectionString;
    }

    public async ValueTask DisposeAsync()
    {
        if (_sharedFactory is not null)
        {
            await _sharedFactory.DisposeAsync();
        }

        if (_sharedDatabases is not null)
        {
            await _sharedDatabases.DisposeAsync();
        }

        SqlConnection.ClearAllPools();

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
