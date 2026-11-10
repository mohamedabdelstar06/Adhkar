using Zad.BuildingBlocks.Application.Identity;

namespace Zad.MigrationRunner.Seeding;

public sealed class RunnerCurrentUser : ICurrentUser
{
    public Guid? UserId => null;

    public ActorType ActorType => ActorType.System;

    public bool IsAuthenticated => true;
}

