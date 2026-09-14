using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Admin.DTOs;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Payments;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Entities.Venues;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class AdminDashboardRepository(
    SEVPMSDbContext dbContext)
    : IAdminDashboardRepository
{
    public async Task<AdminDashboardStatsResponse> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = new AdminDashboardStatsResponse
        {
            TotalUsers = await dbContext.Set<User>()
                .AsNoTracking()
                .CountAsync(cancellationToken),

            ActiveUsers = await dbContext.Set<User>()
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == AccountStatus.Active,
                    cancellationToken),

            SuspendedUsers = await dbContext.Set<User>()
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == AccountStatus.Suspended,
                    cancellationToken),

            TotalVenues = await dbContext.Set<Venue>()
                .AsNoTracking()
                .CountAsync(cancellationToken),

            ActiveVenues = await dbContext.Set<Venue>()
                .AsNoTracking()
                .CountAsync(
                    x => x.IsActive,
                    cancellationToken),

            TotalEvents = await dbContext.Set<Event>()
                .AsNoTracking()
                .CountAsync(cancellationToken),

            PublishedEvents = await dbContext.Set<Event>()
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == EventStatus.Published,
                    cancellationToken),

            PendingBookings = await dbContext.Set<Booking>()
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == BookingStatus.Pending,
                    cancellationToken),

            ConfirmedBookings = await dbContext.Set<Booking>()
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == BookingStatus.Confirmed,
                    cancellationToken),

            SuccessfulPayments = await dbContext.Set<Payment>()
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == PaymentStatus.Successful,
                    cancellationToken),

            SuccessfulRevenue =
                await dbContext.Set<Payment>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Status == PaymentStatus.Successful)
                    .SumAsync(
                        x => (decimal?)x.Amount,
                        cancellationToken)
                ?? 0m,

            GeneratedAtUtc = DateTime.UtcNow
        };

        return result;
    }
}
