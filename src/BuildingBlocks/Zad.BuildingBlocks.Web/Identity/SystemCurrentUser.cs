using Zad.BuildingBlocks.Application.Identity;

namespace Zad.BuildingBlocks.Web.Identity;

public sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? UserId => null;

    public ActorType ActorType => ActorType.System;

    public bool IsAuthenticated => true;
}

