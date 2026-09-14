using SEVPMS.Application.Features.Calendar.DTOs;

namespace SEVPMS.Application.Features.Calendar.Interfaces;

public interface IBookingCalendarService
{
    Task<BookingCalendarExport> GetAsync(
        Guid customerUserId,
        Guid bookingId,
        CancellationToken cancellationToken = default);
}