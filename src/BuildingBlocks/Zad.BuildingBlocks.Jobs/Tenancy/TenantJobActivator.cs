using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Localization;
using Zad.BuildingBlocks.Application.Observability;
using Zad.BuildingBlocks.Application.Tenancy;
using Zad.BuildingBlocks.Domain.Identifiers;

namespace Zad.BuildingBlocks.Jobs.Tenancy;

internal sealed class TenantJobActivator(IServiceScopeFactory scopeFactory) : JobActivator
{
    public override JobActivatorScope BeginScope(JobActivatorContext context)
    {
        var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;

        if (context.GetJobParameter<Guid?>(TenantJobFilter.TenantParameter) is { } tenantId)
        {
            services.GetRequiredService<ITenantContextSetter>().Set(tenantId);
        }

        services.GetRequiredService<ICorrelationContextSetter>().Set(
            context.GetJobParameter<string>(TenantJobFilter.CorrelationParameter) ?? GuidV7.New().ToString("N"),
            causationId: null);

        services.GetRequiredService<ICultureContextSetter>().Set(
            context.GetJobParameter<string>(TenantJobFilter.CultureParameter) ?? "en");

        return new ScopedJobActivatorScope(scope);
    }

    private sealed class ScopedJobActivatorScope(AsyncServiceScope scope) : JobActivatorScope
    {
        public override object Resolve(Type type) => scope.ServiceProvider.GetRequiredService(type);

        public override void DisposeScope() => scope.Dispose();
    }
}
