using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Payments.Interfaces;

namespace SEVPMS.Infrastructure.Providers.Payments;

public sealed class SandboxPaymentCallbackVerifier(
    IConfiguration configuration)
    : ISandboxPaymentCallbackVerifier
{
    public bool Verify(SandboxPaymentCallbackRequest request)
    {
        var secret = configuration["Payments:SandboxWebhookSecret"];

        if (string.IsNullOrWhiteSpace(secret) ||
            string.IsNullOrWhiteSpace(request.Signature))
            return false;

        DateTimeOffset callbackTime;
        try
        {
            callbackTime = DateTimeOffset.FromUnixTimeSeconds(request.TimestampUnix);
        }
        catch
        {
            return false;
        }

        if ((DateTimeOffset.UtcNow - callbackTime).Duration() > TimeSpan.FromMinutes(15))
            return false;

        var expected = Convert.ToHexString(
            HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(secret),
                Encoding.UTF8.GetBytes(Canonical(request))));

        var actual = request.Signature.Trim().ToUpperInvariant();

        if (expected.Length != actual.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected),
            Encoding.ASCII.GetBytes(actual));
    }

    public string HashPayload(SandboxPaymentCallbackRequest request)
        => Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(Canonical(request))));

    private static string Canonical(SandboxPaymentCallbackRequest request)
        => string.Join(
            "|",
            request.PaymentId.ToString("N"),
            request.ProviderReference.Trim(),
            request.Status.Trim().ToUpperInvariant(),
            request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            request.Currency.Trim().ToUpperInvariant(),
            request.TimestampUnix.ToString(CultureInfo.InvariantCulture));
}
