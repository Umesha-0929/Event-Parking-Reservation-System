using SEVPMS.Application.Features.Payments.DTOs;

namespace SEVPMS.Application.Features.Bookings.Interfaces;

public interface IConfirmedBookingCancellationService
{
    Task<RefundResponse> CancelAndRefundAsync(
        Guid customerUserId,
        Guid bookingId,
        string reason,
        CancellationToken cancellationToken = default);
}
