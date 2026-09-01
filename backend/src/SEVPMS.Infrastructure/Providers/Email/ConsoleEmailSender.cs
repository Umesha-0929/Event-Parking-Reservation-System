using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers.Email;

public sealed class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DEV EMAIL] To: {to} | Subject: {subject}");
        return Task.CompletedTask;
    }
}
