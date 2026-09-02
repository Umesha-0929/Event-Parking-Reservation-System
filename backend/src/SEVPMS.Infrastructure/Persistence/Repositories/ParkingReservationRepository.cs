using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class ParkingReservationRepository(
    SEVPMSDbContext dbContext)
    : IParkingReservationRepository
{
    public async Task<ParkingReservation?> GetByIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingReservation>()
            .SingleOrDefaultAsync(
                reservation => reservation.Id == reservationId,
                cancellationToken);
    }

    public async Task<ParkingReservation?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingReservation>()
            .SingleOrDefaultAsync(
                reservation => reservation.BookingId == bookingId,
                cancellationToken);
    }

    public async Task<ParkingReservation?> GetActiveByParkingSlotIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingReservation>()
            .Where(reservation =>
                reservation.ParkingSlotId == parkingSlotId)
            .Where(reservation =>
                reservation.Status == "Reserved" ||
                reservation.Status == "Entered" ||
                reservation.Status == "Parked")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(
        ParkingReservation reservation,
        CancellationToken cancellationToken = default)
    {
        await dbContext
            .Set<ParkingReservation>()
            .AddAsync(
                reservation,
                cancellationToken);
    }

    public void Update(
        ParkingReservation reservation)
    {
        dbContext
            .Set<ParkingReservation>()
            .Update(reservation);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}