using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers.Sms;

public sealed class ConsoleSmsSender : ISmsSender
{
    public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DEV SMS] To: {phoneNumber} | {message}");
        return Task.CompletedTask;
    }
}
