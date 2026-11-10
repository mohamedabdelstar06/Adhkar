using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zad.BuildingBlocks.Application.Messaging;
using Zad.BuildingBlocks.Application.UnitOfWork;
using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Messaging.Registry;

public sealed record Subscription(
    string EventName,
    string HandlerName,
    HandlerLane Lane,
    string ConsumerModule,
    string Schema,
    Type ContextType,
    Type UnitOfWorkType,
    Func<IServiceProvider, Guid, DbContext> CreateContext,
    Func<IServiceProvider, IIntegrationEvent, ExternalEffectContext?, CancellationToken, Task<Result>> Invoke)
{
    public IUnitOfWork UnitOfWork(IServiceProvider provider) =>
        (IUnitOfWork)provider.GetRequiredService(UnitOfWorkType);
}

