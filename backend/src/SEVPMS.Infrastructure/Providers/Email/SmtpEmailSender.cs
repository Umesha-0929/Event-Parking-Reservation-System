using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers.Email;

public sealed class SmtpEmailSender(
    IConfiguration configuration)
    : IEmailSender
{
    public async Task SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:Smtp:Host"]
            ?? throw new InvalidOperationException("SMTP host is not configured.");

        var from = configuration["Email:Smtp:From"]
            ?? throw new InvalidOperationException("SMTP sender is not configured.");

        var port = int.TryParse(configuration["Email:Smtp:Port"], out var parsedPort)
            ? parsedPort
            : 587;

        var enableSsl =
            !bool.TryParse(configuration["Email:Smtp:EnableSsl"], out var parsedSsl) ||
            parsedSsl;
        var username = configuration["Email:Smtp:Username"];
        var password = configuration["Email:Smtp:Password"];

        using var message = new MailMessage(from, to, subject, body);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(
                username,
                password ?? string.Empty);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
