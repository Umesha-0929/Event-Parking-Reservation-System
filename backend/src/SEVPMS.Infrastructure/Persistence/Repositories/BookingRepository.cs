using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(SEVPMSDbContext dbContext) : IBookingRepository
{
    public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        => dbContext.Set<Booking>().FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetByCustomerAsync(Guid customerUserId, CancellationToken cancellationToken = default)
        => await dbContext.Set<Booking>()
            .AsNoTracking()
            .Where(x => x.CustomerUserId == customerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetSeatIdsAsync(Guid bookingId, CancellationToken cancellationToken = default)
        => await dbContext.Set<BookingSeat>()
            .AsNoTracking()
            .Where(x => x.BookingId == bookingId)
            .Select(x => x.SeatId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Booking booking, IReadOnlyCollection<BookingSeat> bookingSeats, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Booking>().AddAsync(booking, cancellationToken);
        await dbContext.Set<BookingSeat>().AddRangeAsync(bookingSeats, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
