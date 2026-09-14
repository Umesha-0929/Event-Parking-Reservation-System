using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Validators;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Parking;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Parking.Services;

public sealed class ParkingReservationService(
    IParkingReservationRepository reservationRepository,
    IBookingRepository bookingRepository,
    ISavedVehicleRepository vehicleRepository)
    : IParkingReservationService
{
    public async Task<ParkingReservationDto?> GetByIdAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await reservationRepository.GetByIdAsync(
            reservationId,
            cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        await EnsureOwnedBookingAsync(
            userId,
            reservation.BookingId,
            cancellationToken);

        return Map(reservation);
    }

    public async Task<ParkingReservationDto> CreateAsync(
        Guid userId,
        CreateParkingReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.BookingId == Guid.Empty)
        {
            throw new ParkingReservationValidationException(
                "Booking is required.");
        }

        if (request.ParkingSlotId == Guid.Empty)
        {
            throw new ParkingReservationValidationException(
                "Parking slot is required.");
        }

        var booking = await EnsureOwnedBookingAsync(
            userId,
            request.BookingId,
            cancellationToken);

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
        {
            throw new ParkingReservationValidationException(
                "Parking can only be reserved for a pending or confirmed booking.");
        }

        var existingForBooking =
            await reservationRepository.GetByBookingIdAsync(
                request.BookingId,
                cancellationToken);

        if (existingForBooking is not null &&
            existingForBooking.Status != ParkingReservationStatuses.Cancelled &&
            existingForBooking.Status != ParkingReservationStatuses.Exited)
        {
            throw new ParkingReservationValidationException(
                "This booking already has an active parking reservation.");
        }

        var slot =
            await reservationRepository.GetParkingSlotByIdAsync(
                request.ParkingSlotId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Parking slot was not found.");

        if (slot.EventId.HasValue &&
            slot.EventId.Value != booking.EventId)
        {
            throw new ParkingReservationValidationException(
                "Parking slot does not belong to the booked event.");
        }

        if (!string.Equals(
                slot.Status,
                "Available",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ParkingReservationValidationException(
                "Parking slot is not available.");
        }

        var activeForSlot =
            await reservationRepository.GetActiveByParkingSlotIdAsync(
                slot.Id,
                cancellationToken);

        if (activeForSlot is not null)
        {
            throw new ParkingReservationValidationException(
                "Parking slot already has an active reservation.");
        }

        Guid? vehicleId = null;
        string registration;

        if (request.VehicleId.HasValue)
        {
            var vehicle =
                await vehicleRepository.GetByIdAsync(
                    request.VehicleId.Value,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    "Saved vehicle was not found.");

            if (vehicle.UserId != userId)
            {
                throw new ForbiddenAccessException(
                    "You do not own the selected vehicle.");
            }

            vehicleId = vehicle.Id;
            registration = vehicle.RegistrationNo;
        }
        else
        {
            registration =
                request.VehicleRegistration?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(registration))
            {
                throw new ParkingReservationValidationException(
                    "Vehicle registration is required when a saved vehicle is not selected.");
            }
        }

        var reservation = new ParkingReservation
        {
            BookingId = booking.Id,
            ParkingSlotId = slot.Id,
            VehicleId = vehicleId,
            VehicleRegSnapshot = registration,
            Status = ParkingReservationStatuses.Reserved,
            ReservedAtUtc = DateTime.UtcNow
        };

        slot.Status = "Reserved";
        slot.UpdatedAtUtc = DateTime.UtcNow;

        await reservationRepository.AddAsync(
            reservation,
            cancellationToken);

        reservationRepository.UpdateParkingSlot(slot);

        await reservationRepository.SaveChangesAsync(
            cancellationToken);

        return Map(reservation);
    }

    public Task<ParkingReservationDto> MarkEnteredAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
        => TransitionAsync(
            userId,
            reservationId,
            ParkingReservationStatuses.Reserved,
            ParkingReservationStatuses.Entered,
            "Occupied",
            cancellationToken);

    public Task<ParkingReservationDto> MarkParkedAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
        => TransitionAsync(
            userId,
            reservationId,
            ParkingReservationStatuses.Entered,
            ParkingReservationStatuses.Parked,
            "Occupied",
            cancellationToken);

    public Task<ParkingReservationDto> MarkExitedAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
        => TransitionAsync(
            userId,
            reservationId,
            ParkingReservationStatuses.Parked,
            ParkingReservationStatuses.Exited,
            "Available",
            cancellationToken);

    public async Task<bool> CancelAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetOwnedReservationAsync(
            userId,
            reservationId,
            cancellationToken);

        if (reservation.Status ==
            ParkingReservationStatuses.Cancelled)
        {
            return true;
        }

        if (reservation.Status !=
            ParkingReservationStatuses.Reserved)
        {
            throw new ParkingReservationValidationException(
                "Only a reserved parking reservation can be cancelled.");
        }

        reservation.Status =
            ParkingReservationStatuses.Cancelled;

        reservation.UpdatedAtUtc =
            DateTime.UtcNow;

        var slot =
            await reservationRepository.GetParkingSlotByIdAsync(
                reservation.ParkingSlotId,
                cancellationToken);

        if (slot is not null)
        {
            slot.Status = "Available";
            slot.UpdatedAtUtc = DateTime.UtcNow;

            reservationRepository.UpdateParkingSlot(slot);
        }

        reservationRepository.Update(reservation);

        await reservationRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> CancelByBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        if (bookingId == Guid.Empty)
        {
            throw new ParkingReservationValidationException(
                "Booking is required.");
        }

        await EnsureOwnedBookingAsync(
            userId,
            bookingId,
            cancellationToken);

        var reservation =
            await reservationRepository.GetByBookingIdAsync(
                bookingId,
                cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        if (reservation.Status ==
                ParkingReservationStatuses.Cancelled ||
            reservation.Status ==
                ParkingReservationStatuses.Exited)
        {
            return true;
        }

        if (reservation.Status !=
            ParkingReservationStatuses.Reserved)
        {
            throw new ParkingReservationValidationException(
                "Only a reserved parking reservation can be released when cancelling a booking.");
        }

        return await CancelAsync(
            userId,
            reservation.Id,
            cancellationToken);
    }

    public async Task<ParkingReservationDto> ScanAsync(
        Guid userId,
        ParkingQrScanRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var reservationId =
            ParseParkingPassCode(
                request.ParkingPassCode);

        var action =
            request.Action?.Trim()
            ?? string.Empty;

        if (action.Equals(
                "Enter",
                StringComparison.OrdinalIgnoreCase))
        {
            return await MarkEnteredAsync(
                userId,
                reservationId,
                cancellationToken);
        }

        if (action.Equals(
                "Exit",
                StringComparison.OrdinalIgnoreCase))
        {
            return await MarkExitedAsync(
                userId,
                reservationId,
                cancellationToken);
        }

        throw new ParkingReservationValidationException(
            "QR action must be Enter or Exit.");
    }

    private async Task<ParkingReservationDto> TransitionAsync(
        Guid userId,
        Guid reservationId,
        string expectedStatus,
        string nextStatus,
        string slotStatus,
        CancellationToken cancellationToken)
    {
        var reservation =
            await GetOwnedReservationAsync(
                userId,
                reservationId,
                cancellationToken);

        if (reservation.Status == nextStatus)
        {
            throw new ParkingReservationValidationException(
                $"Parking reservation is already {nextStatus}.");
        }

        if (reservation.Status != expectedStatus)
        {
            throw new ParkingReservationValidationException(
                $"Parking reservation cannot change from {reservation.Status} to {nextStatus}.");
        }

        reservation.Status = nextStatus;
        reservation.UpdatedAtUtc = DateTime.UtcNow;

        var slot =
            await reservationRepository.GetParkingSlotByIdAsync(
                reservation.ParkingSlotId,
                cancellationToken);

        if (slot is not null)
        {
            slot.Status = slotStatus;
            slot.UpdatedAtUtc = DateTime.UtcNow;

            reservationRepository.UpdateParkingSlot(slot);
        }

        reservationRepository.Update(reservation);

        await reservationRepository.SaveChangesAsync(
            cancellationToken);

        return Map(reservation);
    }

    private async Task<ParkingReservation> GetOwnedReservationAsync(
        Guid userId,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        var reservation =
            await reservationRepository.GetByIdAsync(
                reservationId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Parking reservation was not found.");

        await EnsureOwnedBookingAsync(
            userId,
            reservation.BookingId,
            cancellationToken);

        return reservation;
    }

    private async Task<Booking> EnsureOwnedBookingAsync(
        Guid userId,
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        var booking =
            await bookingRepository.GetByIdAsync(
                bookingId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Booking was not found.");

        if (booking.CustomerUserId != userId)
        {
            throw new ForbiddenAccessException(
                "You do not own this booking.");
        }

        return booking;
    }

    private static Guid ParseParkingPassCode(
        string? passCode)
    {
        var value =
            passCode?.Trim()
            ?? string.Empty;

        const string prefix = "PARK-";

        if (!value.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParse(
                value[prefix.Length..],
                out var reservationId))
        {
            throw new ParkingReservationValidationException(
                "Parking pass code is invalid.");
        }

        return reservationId;
    }

    private static ParkingReservationDto Map(
        ParkingReservation reservation)
        => new()
        {
            Id = reservation.Id,
            BookingId = reservation.BookingId,
            ParkingSlotId = reservation.ParkingSlotId,
            VehicleId = reservation.VehicleId,
            VehicleRegSnapshot =
                reservation.VehicleRegSnapshot,
            Status = reservation.Status,
            ReservedAtUtc =
                reservation.ReservedAtUtc
        };
}