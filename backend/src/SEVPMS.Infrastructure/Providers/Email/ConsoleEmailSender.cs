using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers.Email;

public sealed class ConsoleEmailSender : IEmailSender
{
    public Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine();
        Console.WriteLine("========== DEV EMAIL ==========");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine(body);
        Console.WriteLine("===============================");
        Console.WriteLine();

        return Task.CompletedTask;
    }
}