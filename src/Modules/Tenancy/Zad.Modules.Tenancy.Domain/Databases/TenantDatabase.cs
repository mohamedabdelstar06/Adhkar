using Zad.BuildingBlocks.Contracts.Results;
using Zad.BuildingBlocks.Domain.Entities;

namespace Zad.Modules.Tenancy.Domain.Databases;

public sealed class TenantDatabase : AggregateRoot<DatabaseId>
{
    private TenantDatabase()
    {
    }

    public DatabaseKind Kind { get; private set; }
    public string Name { get; private set; } = default!;
    public string AppConnectionStringProtected { get; private set; } = default!;
    public string MigratorConnectionStringProtected { get; private set; } = default!;
    public DatabaseLifecycle Status { get; private set; }

    public static Result<TenantDatabase> Register(
        DatabaseKind kind,
        string name,
        string appConnectionStringProtected,
        string migratorConnectionStringProtected)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<TenantDatabase>.Failure(TenantDatabaseErrors.InvalidName);
        }

        return Result<TenantDatabase>.Success(new TenantDatabase
        {
            Id = DatabaseId.New(),
            Kind = kind,
            Name = name.Trim(),
            AppConnectionStringProtected = appConnectionStringProtected,
            MigratorConnectionStringProtected = migratorConnectionStringProtected,
            Status = DatabaseLifecycle.Provisioning
        });
    }

    public Result MarkOnline()
    {
        Status = DatabaseLifecycle.Online;
        return Result.Success();
    }

    public Result ReplaceConnectionStrings(string appConnectionStringProtected, string migratorConnectionStringProtected)
    {
        AppConnectionStringProtected = appConnectionStringProtected;
        MigratorConnectionStringProtected = migratorConnectionStringProtected;
        return Result.Success();
    }
}
