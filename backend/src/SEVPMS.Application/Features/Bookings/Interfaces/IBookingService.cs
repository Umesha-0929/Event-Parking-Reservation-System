using SEVPMS.Application.Features.Bookings.DTOs;

namespace SEVPMS.Application.Features.Bookings.Interfaces;

public interface IBookingService
{
    Task<IReadOnlyList<BookingResponse>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    Task<BookingResponse> GetByIdAsync(Guid customerUserId, Guid bookingId, CancellationToken cancellationToken = default);
    Task<BookingResponse> CreateAsync(Guid customerUserId, CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<BookingResponse> CancelAsync(Guid customerUserId, Guid bookingId, CancellationToken cancellationToken = default);
}
