using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Reports.DTOs;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Food;
using SEVPMS.Domain.Entities.Parking;
using SEVPMS.Domain.Entities.Payments;
using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository(
    SEVPMSDbContext dbContext)
    : IReportRepository
{
    public async Task<PlatformReportResponse> GetPlatformAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var users =
            dbContext.Set<User>()
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAtUtc >= fromUtc &&
                    x.CreatedAtUtc < toUtc);

        var events =
            dbContext.Set<Event>()
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAtUtc >= fromUtc &&
                    x.CreatedAtUtc < toUtc);

        var bookings =
            dbContext.Set<Booking>()
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAtUtc >= fromUtc &&
                    x.CreatedAtUtc < toUtc);

        var payments =
            dbContext.Set<Payment>()
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAtUtc >= fromUtc &&
                    x.CreatedAtUtc < toUtc);

        var refunds =
            dbContext.Set<Refund>()
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAtUtc >= fromUtc &&
                    x.CreatedAtUtc < toUtc);

        var attendance =
            dbContext.Set<CheckIn>()
                .AsNoTracking()
                .Where(x =>
                    x.ScannedAtUtc >= fromUtc &&
                    x.ScannedAtUtc < toUtc &&
                    x.Result == CheckInResult.Accepted);

        var parkingReservations =
            dbContext.Set<ParkingReservation>()
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAtUtc >= fromUtc &&
                    x.CreatedAtUtc < toUtc);

        var foodOrders =
            dbContext.Set<FoodOrder>()
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAtUtc >= fromUtc &&
                    x.CreatedAtUtc < toUtc);

        var gross =
            await payments
                .Where(x =>
                    x.Status == PaymentStatus.Successful ||
                    x.Status == PaymentStatus.Refunded)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    cancellationToken)
            ?? 0m;

        var refunded =
            await refunds
                .Where(x =>
                    x.Status == RefundStatus.Successful)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    cancellationToken)
            ?? 0m;

        var foodRevenue =
            await foodOrders
                .SumAsync(
                    x => (decimal?)x.Total,
                    cancellationToken)
            ?? 0m;

        return new PlatformReportResponse
        {
            FromUtc = fromUtc,
            ToUtc = toUtc,

            Users =
                await users.CountAsync(
                    cancellationToken),

            Events =
                await events.CountAsync(
                    cancellationToken),

            PublishedEvents =
                await events.CountAsync(
                    x =>
                        x.Status ==
                        EventStatus.Published,
                    cancellationToken),

            Venues =
                await dbContext.Set<Venue>()
                    .AsNoTracking()
                    .CountAsync(
                        cancellationToken),

            Bookings =
                await bookings.CountAsync(
                    cancellationToken),

            ConfirmedBookings =
                await bookings.CountAsync(
                    x =>
                        x.Status ==
                        BookingStatus.Confirmed,
                    cancellationToken),

            SuccessfulPayments =
                await payments.CountAsync(
                    x =>
                        x.Status ==
                            PaymentStatus.Successful ||
                        x.Status ==
                            PaymentStatus.Refunded,
                    cancellationToken),

            Refunds =
                await refunds.CountAsync(
                    x =>
                        x.Status ==
                        RefundStatus.Successful,
                    cancellationToken),

            Attendance =
                await attendance.CountAsync(
                    cancellationToken),

            ParkingReservations =
                await parkingReservations.CountAsync(
                    cancellationToken),

            FoodOrders =
                await foodOrders.CountAsync(
                    cancellationToken),

            GrossRevenue = gross,

            RefundedAmount = refunded,

            NetRevenue =
                gross - refunded,

            FoodRevenue =
                foodRevenue
        };
    }

    public async Task<OrganizerReportResponse>
        GetOrganizerAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
    {
        var eventIds =
            await dbContext.Set<Event>()
                .AsNoTracking()
                .Where(x =>
                    x.OrganizerUserId ==
                    organizerUserId)
                .Select(x => x.Id)
                .ToListAsync(
                    cancellationToken);

        var confirmedBookingIds =
            await dbContext.Set<Booking>()
                .AsNoTracking()
                .Where(x =>
                    eventIds.Contains(x.EventId) &&
                    x.Status ==
                        BookingStatus.Confirmed)
                .Select(x => x.Id)
                .ToListAsync(
                    cancellationToken);

        var revenue =
            await dbContext.Set<Payment>()
                .AsNoTracking()
                .Where(x =>
                    confirmedBookingIds.Contains(
                        x.BookingId) &&
                    x.Status ==
                        PaymentStatus.Successful)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    cancellationToken)
            ?? 0m;

        var attendance =
            await dbContext.Set<CheckIn>()
                .AsNoTracking()
                .CountAsync(
                    x =>
                        eventIds.Contains(
                            x.EventId) &&
                        x.Result ==
                            CheckInResult.Accepted,
                    cancellationToken);

        var parkingReservations =
            await dbContext.Set<ParkingReservation>()
                .AsNoTracking()
                .CountAsync(
                    x =>
                        confirmedBookingIds.Contains(
                            x.BookingId),
                    cancellationToken);

        var foodOrdersQuery =
            dbContext.Set<FoodOrder>()
                .AsNoTracking()
                .Where(x =>
                    eventIds.Contains(
                        x.EventId));

        var foodOrders =
            await foodOrdersQuery.CountAsync(
                cancellationToken);

        var foodRevenue =
            await foodOrdersQuery
                .SumAsync(
                    x => (decimal?)x.Total,
                    cancellationToken)
            ?? 0m;

        return new OrganizerReportResponse
        {
            OrganizerUserId =
                organizerUserId,

            Events =
                eventIds.Count,

            PublishedEvents =
                await dbContext.Set<Event>()
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.OrganizerUserId ==
                                organizerUserId &&
                            x.Status ==
                                EventStatus.Published,
                        cancellationToken),

            ConfirmedBookings =
                confirmedBookingIds.Count,

            Attendance =
                attendance,

            ParkingReservations =
                parkingReservations,

            FoodOrders =
                foodOrders,

            Revenue =
                revenue,

            FoodRevenue =
                foodRevenue
        };
    }

    public async Task<VenueOwnerReportResponse>
        GetVenueOwnerAsync(
            Guid ownerUserId,
            CancellationToken cancellationToken = default)
    {
        var venueIds =
            await dbContext.Set<Venue>()
                .AsNoTracking()
                .Where(x =>
                    x.OwnerUserId ==
                    ownerUserId)
                .Select(x => x.Id)
                .ToListAsync(
                    cancellationToken);

        var requests =
            dbContext.Set<VenueRentalRequest>()
                .AsNoTracking()
                .Where(x =>
                    venueIds.Contains(
                        x.VenueId));

        return new VenueOwnerReportResponse
        {
            VenueOwnerUserId =
                ownerUserId,

            Venues =
                venueIds.Count,

            RentalRequests =
                await requests.CountAsync(
                    cancellationToken),

            AcceptedRentals =
                await requests.CountAsync(
                    x =>
                        x.Status ==
                        RentalRequestStatus.Accepted,
                    cancellationToken),

            AcceptedRentalValue =
                await requests
                    .Where(x =>
                        x.Status ==
                        RentalRequestStatus.Accepted)
                    .SumAsync(
                        x =>
                            (decimal?)
                                x.OfferedAmount,
                        cancellationToken)
                ?? 0m
        };
    }
}