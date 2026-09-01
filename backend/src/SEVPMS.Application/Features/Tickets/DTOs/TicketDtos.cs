namespace SEVPMS.Application.Features.Tickets.DTOs;
public sealed record IssueTicketsRequest(Guid EventId, IReadOnlyCollection<Guid?> SeatIds);
public sealed record TicketDto(Guid TicketId, string TicketNo, Guid BookingId, Guid EventId, Guid? SeatId, string Status, DateTime IssuedAtUtc, string QrPayload);
public sealed record CheckInTicketRequest(string QrPayload, string Gate);
public sealed record CheckInTicketResponse(bool Succeeded, string Result, string Message, Guid? TicketId = null, string? TicketNo = null, DateTime? ScannedAtUtc = null);
