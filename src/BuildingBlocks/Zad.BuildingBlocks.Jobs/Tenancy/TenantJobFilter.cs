using Hangfire.Client;
using Hangfire.Server;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Jobs.Tenancy;

public sealed class TenantJobFilter : IClientFilter, IServerFilter
{
    internal const string TenantParameter = "zad.tenantId";
    internal const string CorrelationParameter = "zad.correlationId";
    internal const string CultureParameter = "zad.culture";

    public void OnCreating(CreatingContext context)
    {
        var snapshot = JobContextCapture.Current;

        if (snapshot?.TenantId is { } tenantId)
        {
            context.SetJobParameter(TenantParameter, tenantId);
        }

        context.SetJobParameter(CorrelationParameter, snapshot?.CorrelationId ?? GuidV7.New().ToString("N"));
        context.SetJobParameter(CultureParameter, snapshot?.Language ?? "en");
    }

    public void OnCreated(CreatedContext context)
    {
    }

    public void OnPerforming(PerformingContext context)
    {
    }

    public void OnPerformed(PerformedContext context)
    {
    }
}
