using Microsoft.Data.SqlClient;
using Respawn;
using Respawn.Graph;

namespace Zad.TestSupport.Databases;

public sealed class DatabaseResetter
{
    private readonly string _catalogConnectionString;
    private readonly string _sharedConnectionString;
    private readonly Respawner _catalogRespawner;
    private readonly Respawner _sharedRespawner;

    private DatabaseResetter(
        string catalogConnectionString,
        string sharedConnectionString,
        Respawner catalogRespawner,
        Respawner sharedRespawner)
    {
        _catalogConnectionString = catalogConnectionString;
        _sharedConnectionString = sharedConnectionString;
        _catalogRespawner = catalogRespawner;
        _sharedRespawner = sharedRespawner;
    }

    public static async Task<DatabaseResetter> CreateAsync(
        string catalogConnectionString,
        string sharedConnectionString,
        CancellationToken cancellationToken = default)
    {
        await using var catalogConnection = new SqlConnection(catalogConnectionString);
        await catalogConnection.OpenAsync(cancellationToken);

        var catalogRespawner = await Respawner.CreateAsync(
            catalogConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore =
                [
                    new Table("__EFMigrationsHistory"),
                    new Table("migrations", "AppliedMigrations"),
                    new Table("dataprotection", "Keys")
                ]
            });

        await using var sharedConnection = new SqlConnection(sharedConnectionString);
        await sharedConnection.OpenAsync(cancellationToken);

        var sharedRespawner = await Respawner.CreateAsync(
            sharedConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                TablesToIgnore =
                [
                    new Table("__EFMigrationsHistory"),
                    new Table("migrations", "AppliedMigrations")
                ]
            });

        return new DatabaseResetter(catalogConnectionString, sharedConnectionString, catalogRespawner, sharedRespawner);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await using var catalogConnection = new SqlConnection(_catalogConnectionString);
        await catalogConnection.OpenAsync(cancellationToken);
        await _catalogRespawner.ResetAsync(catalogConnection);

        await using var sharedConnection = new SqlConnection(_sharedConnectionString);
        await sharedConnection.OpenAsync(cancellationToken);
        await _sharedRespawner.ResetAsync(sharedConnection);
    }
}

