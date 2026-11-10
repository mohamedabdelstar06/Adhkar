using Microsoft.Extensions.Logging;
using Zad.BuildingBlocks.Application.Email;

namespace Zad.BuildingBlocks.Notifications.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email queued to {Recipient} with subject {Subject}", message.To, message.Subject);
        return Task.CompletedTask;
    }
}

