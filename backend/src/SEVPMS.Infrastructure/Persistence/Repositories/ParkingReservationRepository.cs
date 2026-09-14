using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Validators;
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
            .OrderByDescending(reservation => reservation.ReservedAtUtc)
            .FirstOrDefaultAsync(
                reservation => reservation.BookingId == bookingId,
                cancellationToken);
    }

    public async Task<ParkingReservation?> GetActiveByParkingSlotIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingReservation>()
            .Where(reservation => reservation.ParkingSlotId == parkingSlotId)
            .Where(reservation =>
                reservation.Status == "Reserved" ||
                reservation.Status == "Entered" ||
                reservation.Status == "Parked")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ParkingSlot?> GetParkingSlotByIdAsync(
        Guid parkingSlotId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<ParkingSlot>()
            .SingleOrDefaultAsync(
                slot => slot.Id == parkingSlotId,
                cancellationToken);
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

    public void Update(ParkingReservation reservation)
    {
        dbContext
            .Set<ParkingReservation>()
            .Update(reservation);
    }

    public void UpdateParkingSlot(ParkingSlot parkingSlot)
    {
        dbContext
            .Set<ParkingSlot>()
            .Update(parkingSlot);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ParkingReservationConflictException(
                "Parking slot availability changed because another customer reserved or updated it. Refresh the parking layout and try again.",
                exception);
        }
    }
}