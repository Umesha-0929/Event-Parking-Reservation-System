using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Reports.DTOs;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Payments;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository(SEVPMSDbContext dbContext) : IReportRepository
{
    public async Task<PlatformReportResponse> GetPlatformAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var users = dbContext.Set<User>().AsNoTracking()
            .Where(x => x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc < toUtc);
        var events = dbContext.Set<Event>().AsNoTracking()
            .Where(x => x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc < toUtc);
        var bookings = dbContext.Set<Booking>().AsNoTracking()
            .Where(x => x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc < toUtc);
        var payments = dbContext.Set<Payment>().AsNoTracking()
            .Where(x => x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc < toUtc);
        var refunds = dbContext.Set<Refund>().AsNoTracking()
            .Where(x => x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc < toUtc);

        var gross = await payments
            .Where(x => x.Status == PaymentStatus.Successful ||
                        x.Status == PaymentStatus.Refunded)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var refunded = await refunds
            .Where(x => x.Status == RefundStatus.Successful)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        return new PlatformReportResponse
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Users = await users.CountAsync(cancellationToken),
            Events = await events.CountAsync(cancellationToken),
            PublishedEvents = await events.CountAsync(
                x => x.Status == EventStatus.Published,
                cancellationToken),
            Venues = await dbContext.Set<Venue>()
                .AsNoTracking()
                .CountAsync(cancellationToken),
            Bookings = await bookings.CountAsync(cancellationToken),
            ConfirmedBookings = await bookings.CountAsync(
                x => x.Status == BookingStatus.Confirmed,
                cancellationToken),
            SuccessfulPayments = await payments.CountAsync(
                x => x.Status == PaymentStatus.Successful ||
                     x.Status == PaymentStatus.Refunded,
                cancellationToken),
            Refunds = await refunds.CountAsync(
                x => x.Status == RefundStatus.Successful,
                cancellationToken),
            GrossRevenue = gross,
            RefundedAmount = refunded,
            NetRevenue = gross - refunded
        };
    }

    public async Task<OrganizerReportResponse> GetOrganizerAsync(
        Guid organizerUserId,
        CancellationToken cancellationToken = default)
    {
        var eventIds = await dbContext.Set<Event>()
            .AsNoTracking()
            .Where(x => x.OrganizerUserId == organizerUserId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var confirmedBookingIds = await dbContext.Set<Booking>()
            .AsNoTracking()
            .Where(x => eventIds.Contains(x.EventId) &&
                        x.Status == BookingStatus.Confirmed)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var revenue = await dbContext.Set<Payment>()
            .AsNoTracking()
            .Where(x => confirmedBookingIds.Contains(x.BookingId) &&
                        x.Status == PaymentStatus.Successful)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        return new OrganizerReportResponse
        {
            OrganizerUserId = organizerUserId,
            Events = eventIds.Count,
            PublishedEvents = await dbContext.Set<Event>()
                .AsNoTracking()
                .CountAsync(
                    x => x.OrganizerUserId == organizerUserId &&
                         x.Status == EventStatus.Published,
                    cancellationToken),
            ConfirmedBookings = confirmedBookingIds.Count,
            Revenue = revenue
        };
    }

    public async Task<VenueOwnerReportResponse> GetVenueOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default)
    {
        var venueIds = await dbContext.Set<Venue>()
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var requests = dbContext.Set<VenueRentalRequest>()
            .AsNoTracking()
            .Where(x => venueIds.Contains(x.VenueId));

        return new VenueOwnerReportResponse
        {
            VenueOwnerUserId = ownerUserId,
            Venues = venueIds.Count,
            RentalRequests = await requests.CountAsync(cancellationToken),
            AcceptedRentals = await requests.CountAsync(
                x => x.Status == RentalRequestStatus.Accepted,
                cancellationToken),
            AcceptedRentalValue = await requests
                .Where(x => x.Status == RentalRequestStatus.Accepted)
                .SumAsync(x => (decimal?)x.OfferedAmount, cancellationToken) ?? 0m
        };
    }
}
