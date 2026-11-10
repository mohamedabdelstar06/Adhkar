namespace Zad.MigrationRunner.Execution;

public sealed class RunnerLog
{
    private readonly TextWriter _output = Console.Out;
    private readonly TimeProvider _clock = TimeProvider.System;

    public void Info(string message) => Write("INFO", message);

    public void Warn(string message) => Write("WARN", message);

    public void Error(string message) => Write("ERROR", message);

    public void Script(string script) => _output.WriteLine(script);

    private void Write(string level, string message) =>
        _output.WriteLine($"{_clock.GetUtcNow():yyyy-MM-ddTHH:mm:ss.fffZ} [{level}] {message}");
}
