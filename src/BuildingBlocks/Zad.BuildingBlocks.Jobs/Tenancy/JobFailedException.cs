using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Jobs.Tenancy;

public sealed class JobFailedException(Error error) : Exception($"{error.Code}: {error.Message}")
{
    public Error Error { get; } = error;
}
