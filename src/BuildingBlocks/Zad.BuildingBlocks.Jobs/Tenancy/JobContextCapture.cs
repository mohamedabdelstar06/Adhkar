namespace Zad.BuildingBlocks.Jobs.Tenancy;

internal static class JobContextCapture
{
    private static readonly AsyncLocal<JobContextSnapshot?> Snapshot = new();

    internal static JobContextSnapshot? Current => Snapshot.Value;

    internal static IDisposable Begin(JobContextSnapshot snapshot)
    {
        var previous = Snapshot.Value;
        Snapshot.Value = snapshot;
        return new Restore(previous);
    }

    private sealed class Restore(JobContextSnapshot? previous) : IDisposable
    {
        public void Dispose() => Snapshot.Value = previous;
    }
}

