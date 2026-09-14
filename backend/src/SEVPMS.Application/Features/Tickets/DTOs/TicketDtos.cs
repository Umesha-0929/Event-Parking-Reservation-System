namespace SEVPMS.Application.Features.Tickets.DTOs;
public sealed record IssueTicketsRequest(Guid EventId, IReadOnlyCollection<Guid?> SeatIds);
public sealed record TicketDto(Guid TicketId, string TicketNo, Guid BookingId, Guid EventId, Guid? SeatId, string Status, DateTime IssuedAtUtc, string QrPayload);
public sealed record CheckInTicketRequest(string QrPayload, string Gate);
public sealed record CheckInTicketResponse(bool Succeeded, string Result, string Message, Guid? TicketId = null, string? TicketNo = null, DateTime? ScannedAtUtc = null);

public sealed record CustomerTicketSummaryDto(
    Guid TicketId,
    string TicketNo,
    Guid BookingId,
    string BookingNumber,
    Guid EventId,
    string EventName,
    string VenueName,
    Guid? SeatId,
    string? RowLabel,
    string? SeatNumber,
    string Status,
    DateTime IssuedAtUtc,
    DateTime? CheckedInAtUtc,
    bool IsAccessible,
    string QrPayload);
