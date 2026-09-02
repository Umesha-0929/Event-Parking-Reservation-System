using SEVPMS.Domain.Entities.Bookings;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetByCustomerAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetSeatIdsAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task AddAsync(Booking booking, IReadOnlyCollection<BookingSeat> bookingSeats, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
