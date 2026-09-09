using SEVPMS.Application.Features.Bookings.DTOs;

namespace SEVPMS.Application.Features.Bookings.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<BookingResponse>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<BookingResponse>> GetMinePageAsync(
        Guid customerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetMineAsync(customerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<BookingResponse> GetByIdAsync(Guid customerUserId, Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingResponse> CreateAsync(Guid customerUserId, CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingResponse> CancelAsync(Guid customerUserId, Guid bookingId, CancellationToken cancellationToken = default);
}
