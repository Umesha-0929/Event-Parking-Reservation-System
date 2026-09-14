using System.Data;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Common.Paging;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Entities.Seats;
namespace SEVPMS.Infrastructure.Persistence.Repositories.Tickets;
public sealed class EfTicketRepository(SEVPMSDbContext db) : ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default) => await db.Set<Ticket>().AsNoTracking().Where(t => t.BookingId == bookingId).OrderBy(t => t.TicketNo).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CustomerTicketSummaryRow>> GetForCustomerAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default)
    {
        var query =
            from ticket in db.Set<Ticket>().AsNoTracking()
            join booking in db.Set<Booking>().AsNoTracking() on ticket.BookingId equals booking.Id
            join eventEntity in db.Set<Event>().AsNoTracking() on ticket.EventId equals eventEntity.Id
            join venue in db.Set<Venue>().AsNoTracking() on eventEntity.VenueId equals venue.Id
            join seatEntity in db.Set<Seat>().AsNoTracking() on ticket.SeatId equals (Guid?)seatEntity.Id into seatJoin
            from seat in seatJoin.DefaultIfEmpty()
            where booking.CustomerUserId == customerUserId
            orderby ticket.IssuedAtUtc descending
            select new CustomerTicketSummaryRow(
                ticket,
                booking.BookingNumber,
                eventEntity.Title,
                venue.Name,
                seat == null ? null : seat.RowLabel,
                seat == null ? null : seat.SeatNumber,
                seat != null && seat.IsAccessible);

        return await query.ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<CustomerTicketSummaryRow>> GetForCustomerPageAsync(
        Guid customerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (_, size, skip) = PagingRules.Normalize(page, pageSize);
        var query =
            from ticket in db.Set<Ticket>().AsNoTracking()
            join booking in db.Set<Booking>().AsNoTracking() on ticket.BookingId equals booking.Id
            join eventEntity in db.Set<Event>().AsNoTracking() on ticket.EventId equals eventEntity.Id
            join venue in db.Set<Venue>().AsNoTracking() on eventEntity.VenueId equals venue.Id
            join seatEntity in db.Set<Seat>().AsNoTracking() on ticket.SeatId equals (Guid?)seatEntity.Id into seatJoin
            from seat in seatJoin.DefaultIfEmpty()
            where booking.CustomerUserId == customerUserId
            orderby ticket.IssuedAtUtc descending
            select new CustomerTicketSummaryRow(
                ticket, booking.BookingNumber, eventEntity.Title, venue.Name,
                seat == null ? null : seat.RowLabel,
                seat == null ? null : seat.SeatNumber,
                seat != null && seat.IsAccessible);

        return await query.Skip(skip).Take(size).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> AddIfBookingHasNoneAsync(Guid bookingId, IReadOnlyCollection<Ticket> tickets, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var existing = await db.Set<Ticket>().Where(t => t.BookingId == bookingId).ToListAsync(cancellationToken);
        if (existing.Count > 0) { await tx.CommitAsync(cancellationToken); return existing; }
        db.Set<Ticket>().AddRange(tickets); await db.SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken); return tickets.ToArray();
    }
    public async Task<TicketCheckInAttempt> TryCheckInAsync(Guid ticketId, string qrTokenHash, Guid eventId, Guid scannedByUserId, string gate, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var ticket = await db.Set<Ticket>().FirstOrDefaultAsync(t => t.Id == ticketId && t.QrTokenHash == qrTokenHash, cancellationToken);
        if (ticket is null) { await tx.RollbackAsync(cancellationToken); return new(null, CheckInResult.Invalid, nowUtc); }
        var result = ticket.EventId != eventId ? CheckInResult.WrongEvent : ticket.Status switch { TicketStatus.CheckedIn => CheckInResult.Duplicate, TicketStatus.Cancelled => CheckInResult.Cancelled, TicketStatus.Voided => CheckInResult.Voided, TicketStatus.Active => CheckInResult.Accepted, _ => CheckInResult.Invalid };
        db.Set<CheckIn>().Add(new CheckIn { TicketId = ticket.Id, EventId = eventId, ScannedByUserId = scannedByUserId, Gate = gate, ScannedAtUtc = nowUtc, Result = result, Detail = result == CheckInResult.Accepted ? null : result.ToString(), CreatedAtUtc = nowUtc });
        if (result == CheckInResult.Accepted) { ticket.Status = TicketStatus.CheckedIn; ticket.CheckedInAtUtc = nowUtc; ticket.UpdatedAtUtc = nowUtc; }
        await db.SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken); return new(ticket, result, nowUtc);
    }
    public Task<Ticket?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default) => db.Set<Ticket>().AsNoTracking().FirstOrDefaultAsync(t => t.TicketNo == ticketNo, cancellationToken);
    public async Task<bool> CancelAsync(string ticketNo, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var t = await db.Set<Ticket>().FirstOrDefaultAsync(x => x.TicketNo == ticketNo, cancellationToken); if (t is null || t.Status != TicketStatus.Active) return false; t.Status = TicketStatus.Cancelled; t.UpdatedAtUtc = nowUtc; await db.SaveChangesAsync(cancellationToken); return true;
    }
}
