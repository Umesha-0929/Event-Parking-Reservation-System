using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers.Sms;

/// <summary>
/// Generic HTTPS SMS adapter.
/// Configure Sms:Http:Endpoint and optionally Sms:Http:ApiKey / ApiKeyHeader.
/// Expected provider body: { "to": "...", "message": "..." }.
/// </summary>
public sealed class HttpSmsSender(
    IConfiguration configuration)
    : ISmsSender
{
    public async Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        var endpoint = configuration["Sms:Http:Endpoint"]
            ?? throw new InvalidOperationException("SMS HTTP endpoint is not configured.");

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "SMS HTTP endpoint must be an absolute HTTPS URL.");
        }

        using var client = new HttpClient();

        var apiKey = configuration["Sms:Http:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var header = configuration["Sms:Http:ApiKeyHeader"] ?? "Authorization";
            client.DefaultRequestHeaders.TryAddWithoutValidation(header, apiKey);
        }

        using var response = await client.PostAsJsonAsync(
            uri,
            new
            {
                to = phoneNumber,
                message
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
