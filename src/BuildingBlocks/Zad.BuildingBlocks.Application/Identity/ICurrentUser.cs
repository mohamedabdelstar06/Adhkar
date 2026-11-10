namespace Zad.BuildingBlocks.Application.Identity;

public interface ICurrentUser
{
    public Guid? UserId { get; }
    public ActorType ActorType { get; }
    public bool IsAuthenticated { get; }
}
