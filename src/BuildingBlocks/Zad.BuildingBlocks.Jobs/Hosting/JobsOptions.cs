namespace Zad.BuildingBlocks.Jobs.Hosting;

public sealed class JobsOptions
{
    public const string SectionName = "Jobs";

    public bool ServerEnabled { get; set; } = true;

    public int WorkerCount { get; set; } = Math.Min(Environment.ProcessorCount * 2, 20);

    public string[] Queues { get; set; } = ["default"];

    public int DefaultRetryAttempts { get; set; } = 5;
}
