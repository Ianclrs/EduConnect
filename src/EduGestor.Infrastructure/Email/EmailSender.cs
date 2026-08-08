using Microsoft.Extensions.Logging;

namespace EduGestor.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string body);
}

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string toName, string subject, string body)
    {
#pragma warning disable CA1848, CA1873
        _logger.LogInformation(
            "[EMAIL] To: {Email} ({Name}) | Subject: {Subject} | Body: {Body}",
            toEmail, toName, subject, body);
#pragma warning restore CA1848, CA1873
        return Task.CompletedTask;
    }
}
