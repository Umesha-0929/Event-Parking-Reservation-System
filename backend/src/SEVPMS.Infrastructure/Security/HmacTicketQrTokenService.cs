using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Features.Tickets.Interfaces;
namespace SEVPMS.Infrastructure.Security;
public sealed class HmacTicketQrTokenService : ITicketQrTokenService
{
    private readonly byte[] _key;
    public HmacTicketQrTokenService(IConfiguration configuration)
    {
        var value = configuration["Klegar:TicketQrSigningKey"];
        if (string.IsNullOrWhiteSpace(value) || value.Length < 32) throw new InvalidOperationException("Klegar:TicketQrSigningKey must be configured with at least 32 characters. Use an environment variable; do not commit real secrets.");
        _key = Encoding.UTF8.GetBytes(value);
    }
    public string CreatePayload(Guid ticketId)
    {
        var id = ticketId.ToString("N"); using var hmac = new HMACSHA256(_key); var signature = Base64Url(hmac.ComputeHash(Encoding.UTF8.GetBytes(id))); return $"SEVPMS.TICKET.{id}.{signature}";
    }
    public string HashPayload(string payload) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    public bool TryValidatePayload(string payload, out Guid ticketId)
    {
        ticketId = Guid.Empty; var parts = payload.Split('.', StringSplitOptions.RemoveEmptyEntries); if (parts.Length != 4 || parts[0] != "SEVPMS" || parts[1] != "TICKET" || !Guid.TryParseExact(parts[2], "N", out ticketId)) return false;
        var expected = CreatePayload(ticketId); return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(payload));
    }
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
