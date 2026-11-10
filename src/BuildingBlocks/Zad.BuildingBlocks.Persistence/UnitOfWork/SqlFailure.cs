using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Persistence.UnitOfWork;

public static class SqlFailure
{
    private const int DuplicateKey = 2601;
    private const int UniqueConstraint = 2627;

    public static Error? Classify(
        Exception exception,
        IReadOnlyDictionary<string, Error> uniqueIndexErrors)
    {
        if (exception is not DbUpdateException { InnerException: SqlException sql })
        {
            return null;
        }

        if (sql.Number is not (DuplicateKey or UniqueConstraint))
        {
            return null;
        }

        foreach (var (indexName, error) in uniqueIndexErrors)
        {
            if (sql.Message.Contains(indexName, StringComparison.OrdinalIgnoreCase))
            {
                return error;
            }
        }

        return null;
    }
}
