using Zad.Modules.Tenancy.Domain.Databases;

namespace Zad.Modules.Tenancy.Application.Databases;

public sealed record RegisterDatabaseRequest(
    string Name,
    DatabaseKind Kind,
    string AppConnectionString,
    string MigratorConnectionString);

