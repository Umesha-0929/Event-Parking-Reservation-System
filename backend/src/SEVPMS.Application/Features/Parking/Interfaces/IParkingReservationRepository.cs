using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Application.Features.Parking.Interfaces;

public interface IParkingReservationRepository
{
    Task<ParkingReservation?> GetByIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservation?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<ParkingReservation?> GetActiveByParkingSlotIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default);

    Task<ParkingSlot?> GetParkingSlotByIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ParkingReservation reservation,
        CancellationToken cancellationToken = default);

    void Update(ParkingReservation reservation);

    void UpdateParkingSlot(ParkingSlot parkingSlot);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
