namespace Zad.BuildingBlocks.Application.Email;

public interface IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

