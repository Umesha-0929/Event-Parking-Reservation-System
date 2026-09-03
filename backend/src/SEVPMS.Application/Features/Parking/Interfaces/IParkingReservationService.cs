using SEVPMS.Application.Features.Parking.DTOs;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingReservationService
{
    Task<ParkingReservationDto?> GetByIdAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto> CreateAsync(
        Guid userId,
        CreateParkingReservationRequest request,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto> MarkEnteredAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto> MarkParkedAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto> MarkExitedAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelByBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto> ScanAsync(
        Guid userId,
        ParkingQrScanRequest request,
        CancellationToken cancellationToken = default);
}