namespace Zad.BuildingBlocks.Jobs.Scheduling;

public sealed record RecurringJobDefinition(string Id, Type JobType, string Cron);
