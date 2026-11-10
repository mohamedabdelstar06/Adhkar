using Zad.BuildingBlocks.Application.Messaging;
using Zad.BuildingBlocks.Contracts.Messaging;
using Zad.BuildingBlocks.Contracts.Results;

namespace Zad.BuildingBlocks.Messaging.Registry;

public sealed record HandlerDescriptor(
    string EventName,
    string HandlerName,
    HandlerLane Lane,
    string ConsumerModule,
    Func<IServiceProvider, IIntegrationEvent, ExternalEffectContext?, CancellationToken, Task<Result>> Invoke);

