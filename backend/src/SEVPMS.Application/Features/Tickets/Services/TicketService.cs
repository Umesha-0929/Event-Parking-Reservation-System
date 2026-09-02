using System.Security.Cryptography;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Application.Features.Tickets.Services;
public sealed class TicketService(ITicketRepository repository, ITicketQrTokenService qrTokens, ITicketRealtimeNotifier realtime, TimeProvider timeProvider) : ITicketService
{
    public async Task<IReadOnlyList<TicketDto>> IssueAsync(Guid bookingId, IssueTicketsRequest request, CancellationToken cancellationToken = default)
    {
        if (bookingId == Guid.Empty || request.EventId == Guid.Empty) throw new ArgumentException("Booking and event are required.");
        var existing = await repository.GetByBookingAsync(bookingId, cancellationToken);
        if (existing.Count > 0) return existing.Select(Map).ToArray();
        var seats = request.SeatIds?.ToArray() ?? Array.Empty<Guid?>();
        if (seats.Length == 0) seats = new Guid?[] { null };
        if (seats.Where(x => x.HasValue).GroupBy(x => x).Any(g => g.Count() > 1)) throw new ArgumentException("Duplicate seat ids are not allowed.");
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tickets = seats.Select(seatId =>
        {
            var ticket = new Ticket { BookingId = bookingId, EventId = request.EventId, SeatId = seatId, TicketNo = NewTicketNo(now), Status = TicketStatus.Active, IssuedAtUtc = now, CreatedAtUtc = now };
            ticket.QrTokenHash = qrTokens.HashPayload(qrTokens.CreatePayload(ticket.Id));
            return ticket;
        }).ToArray();
        var saved = await repository.AddIfBookingHasNoneAsync(bookingId, tickets, cancellationToken);
        return saved.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<TicketDto>> GetForBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) => (await repository.GetByBookingAsync(bookingId, cancellationToken)).Select(Map).ToArray();
    public async Task<TicketDto?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default)
    {
        var t = await repository.GetByTicketNoAsync(ticketNo.Trim(), cancellationToken);
        return t is null ? null : Map(t);
    }

    public async Task<CheckInTicketResponse> CheckInAsync(Guid eventId, Guid scannerUserId, CheckInTicketRequest request, CancellationToken cancellationToken = default)
    {
        if (eventId == Guid.Empty || scannerUserId == Guid.Empty || string.IsNullOrWhiteSpace(request.QrPayload)) return new(false, "Invalid", "A valid event, scanner and QR payload are required.");
        if (!qrTokens.TryValidatePayload(request.QrPayload.Trim(), out var ticketId)) return new(false, "Invalid", "The QR ticket is invalid.");
        var hash = qrTokens.HashPayload(request.QrPayload.Trim());
        var attempt = await repository.TryCheckInAsync(ticketId, hash, eventId, scannerUserId, request.Gate?.Trim() ?? string.Empty, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        var ticket = attempt.Ticket;
        var success = attempt.Result == CheckInResult.Accepted;
        var message = attempt.Result switch { CheckInResult.Accepted => "Ticket checked in successfully.", CheckInResult.Duplicate => "This ticket has already been checked in.", CheckInResult.WrongEvent => "This ticket belongs to a different event.", CheckInResult.Cancelled => "This ticket is cancelled.", CheckInResult.Voided => "This ticket is voided.", _ => "The QR ticket is invalid." };
        if (ticket is not null) await realtime.PublishCheckInChangedAsync(eventId, ticket.Id, attempt.Result.ToString(), request.Gate ?? string.Empty, attempt.ScannedAtUtc, cancellationToken);
        return new(success, attempt.Result.ToString(), message, ticket?.Id, ticket?.TicketNo, attempt.ScannedAtUtc);
    }

    public Task<bool> CancelAsync(string ticketNo, CancellationToken cancellationToken = default) => repository.CancelAsync(ticketNo.Trim(), timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

    private TicketDto Map(Ticket t) => new(t.Id, t.TicketNo, t.BookingId, t.EventId, t.SeatId, t.Status.ToString(), t.IssuedAtUtc, qrTokens.CreatePayload(t.Id));
    private static string NewTicketNo(DateTime now) => $"TKT-{now:yyyyMMdd}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(4))}";
}
