using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Messaging.Retries;

public sealed class RetryPolicy(
    int maxAttempts = 10,
    TimeSpan? baseDelay = null,
    TimeSpan? maxDelay = null)
{
    private readonly TimeSpan _baseDelay = baseDelay ?? TimeSpan.FromSeconds(2);
    private readonly TimeSpan _maxDelay = maxDelay ?? TimeSpan.FromMinutes(10);

    public int MaxAttempts { get; } = maxAttempts;

    public bool IsExhausted(int attemptCount) => attemptCount >= MaxAttempts;

    public DateTimeOffset NextAttempt(DateTimeOffset now, int attemptCount)
    {
        var exponent = Math.Min(attemptCount - 1, 20);
        var delay = TimeSpan.FromTicks(_baseDelay.Ticks * (long)Math.Pow(2, exponent));

        if (delay > _maxDelay)
        {
            delay = _maxDelay;
        }

        var jitter = 0.8 + (Random.Shared.NextDouble() * 0.4);

        return now.AddTicks((long)(delay.Ticks * jitter));
    }

    public static string Describe(Result result) =>
        result.IsFailure ? result.Error.Code : "unknown";

    public static string Describe(Exception exception)
    {
        var text = $"{exception.GetType().Name}: {exception.Message}";

        return text.Length <= 2000 ? text : text[..2000];
    }
}
