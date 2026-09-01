using System.Security.Cryptography;
using System.Text;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Features.Tickets.Services;
using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
namespace SEVPMS.UnitTests.Tickets;
public sealed class TicketServiceTests
{
    [Fact]
    public async Task CheckIn_ReturnsDuplicate_WhenTicketWasAlreadyUsed()
    {
        var ticket = new Ticket { BookingId = Guid.NewGuid(), EventId = Guid.NewGuid(), TicketNo = "TKT-1", Status = TicketStatus.CheckedIn, IssuedAtUtc = DateTime.UtcNow };
        var qr = new FakeQr(); var payload = qr.CreatePayload(ticket.Id); ticket.QrTokenHash = qr.HashPayload(payload);
        var repo = new FakeRepo { Attempt = new(ticket, CheckInResult.Duplicate, DateTime.UtcNow) };
        var service = new TicketService(repo, qr, new FakeRealtime(), TimeProvider.System);
        var result = await service.CheckInAsync(ticket.EventId, Guid.NewGuid(), new CheckInTicketRequest(payload, "Gate 1"));
        Assert.False(result.Succeeded); Assert.Equal("Duplicate", result.Result);
    }

    [Fact]
    public async Task Issue_IsIdempotent_WhenBookingAlreadyHasTickets()
    {
        var ticket = new Ticket { BookingId = Guid.NewGuid(), EventId = Guid.NewGuid(), TicketNo = "TKT-EXIST", Status = TicketStatus.Active, IssuedAtUtc = DateTime.UtcNow };
        var qr = new FakeQr(); ticket.QrTokenHash = qr.HashPayload(qr.CreatePayload(ticket.Id));
        var repo = new FakeRepo { Existing = new[] { ticket } };
        var service = new TicketService(repo, qr, new FakeRealtime(), TimeProvider.System);
        var result = await service.IssueAsync(ticket.BookingId, new IssueTicketsRequest(ticket.EventId, new Guid?[] { Guid.NewGuid() }));
        Assert.Single(result); Assert.Equal("TKT-EXIST", result[0].TicketNo);
    }

    private sealed class FakeRealtime : ITicketRealtimeNotifier { public Task PublishCheckInChangedAsync(Guid eventId, Guid ticketId, string state, string gate, DateTime scannedAtUtc, CancellationToken cancellationToken = default) => Task.CompletedTask; }
    private sealed class FakeQr : ITicketQrTokenService
    {
        public string CreatePayload(Guid ticketId) => $"T.{ticketId:N}";
        public string HashPayload(string payload) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        public bool TryValidatePayload(string payload, out Guid ticketId) { ticketId = Guid.Empty; var p = payload.Split('.'); return p.Length == 2 && Guid.TryParseExact(p[1], "N", out ticketId); }
    }
    private sealed class FakeRepo : ITicketRepository
    {
        public IReadOnlyList<Ticket> Existing { get; set; } = Array.Empty<Ticket>();
        public TicketCheckInAttempt Attempt { get; set; } = new(null, CheckInResult.Invalid, DateTime.UtcNow);
        public Task<IReadOnlyList<Ticket>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) => Task.FromResult(Existing);
        public Task<IReadOnlyList<Ticket>> AddIfBookingHasNoneAsync(Guid bookingId, IReadOnlyCollection<Ticket> tickets, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Ticket>>(tickets.ToArray());
        public Task<TicketCheckInAttempt> TryCheckInAsync(Guid ticketId, string qrTokenHash, Guid eventId, Guid scannedByUserId, string gate, DateTime nowUtc, CancellationToken cancellationToken = default) => Task.FromResult(Attempt);
        public Task<Ticket?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default) => Task.FromResult<Ticket?>(Existing.FirstOrDefault());
        public Task<bool> CancelAsync(string ticketNo, DateTime nowUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
