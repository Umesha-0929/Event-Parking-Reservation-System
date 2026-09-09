using SEVPMS.Domain.Entities.Bookings;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetByCustomerAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<Booking>> GetByCustomerPageAsync(
        Guid customerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetByCustomerAsync(customerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<IReadOnlyList<Guid>> GetSeatIdsAsync(Guid bookingId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> GetSeatIdsByBookingIdsAsync(
        IReadOnlyCollection<Guid> bookingIds,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, IReadOnlyList<Guid>>();
        foreach (var bookingId in bookingIds)
        {
            result[bookingId] = await GetSeatIdsAsync(bookingId, cancellationToken);
        }
        return result;
    }
    Task AddAsync(Booking booking, IReadOnlyCollection<BookingSeat> bookingSeats, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
