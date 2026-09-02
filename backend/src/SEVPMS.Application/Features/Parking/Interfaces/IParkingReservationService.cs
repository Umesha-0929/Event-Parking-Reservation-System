using SEVPMS.Application.Features.Parking.DTOs;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingReservationService
{
    Task<ParkingReservationDto?> GetByIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto?> CreateAsync(
        Guid userId,
        CreateParkingReservationRequest request,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto?> MarkEnteredAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto?> MarkParkedAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservationDto?> MarkExitedAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);
}