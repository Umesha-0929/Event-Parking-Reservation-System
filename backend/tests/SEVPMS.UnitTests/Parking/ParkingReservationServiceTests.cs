using SEVPMS.Application.Features.Parking;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Services;
using SEVPMS.Application.Features.Parking.Validators;
using SEVPMS.Application.Features.Vehicles.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Parking;
using SEVPMS.Domain.Entities.Vehicles;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Parking;

public sealed class ParkingReservationServiceTests
{
    [Fact]
    public async Task CreateAsync_WithPendingBooking_CreatesReservation()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = new Booking
        {
            CustomerUserId = userId,
            EventId = eventId,
            Status = BookingStatus.Pending
        };

        var slot = new ParkingSlot
        {
            ParkingZoneId = Guid.NewGuid(),
            EventId = eventId,
            SlotCode = "A-01",
            Status = "Available"
        };

        var reservationRepository =
            new FakeParkingReservationRepository(slot);

        var service = new ParkingReservationService(
            reservationRepository,
            new FakeBookingRepository(booking),
            new FakeSavedVehicleRepository());

        var request = new CreateParkingReservationRequest
        {
            BookingId = booking.Id,
            ParkingSlotId = slot.Id,
            VehicleRegistration = "CAB-1234"
        };

        var result = await service.CreateAsync(
            userId,
            request,
            CancellationToken.None);

        Assert.Equal(
            ParkingReservationStatuses.Reserved,
            result.Status);

        Assert.Equal(
            booking.Id,
            result.BookingId);

        Assert.Equal(
            slot.Id,
            result.ParkingSlotId);

        Assert.Equal(
            "Reserved",
            slot.Status);

        Assert.NotNull(
            reservationRepository.AddedReservation);
    }

    [Fact]
    public async Task CreateAsync_WithConfirmedBooking_CreatesReservation()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = new Booking
        {
            CustomerUserId = userId,
            EventId = eventId,
            Status = BookingStatus.Confirmed
        };

        var slot = new ParkingSlot
        {
            ParkingZoneId = Guid.NewGuid(),
            EventId = eventId,
            SlotCode = "A-02",
            Status = "Available"
        };

        var service = new ParkingReservationService(
            new FakeParkingReservationRepository(slot),
            new FakeBookingRepository(booking),
            new FakeSavedVehicleRepository());

        var request = new CreateParkingReservationRequest
        {
            BookingId = booking.Id,
            ParkingSlotId = slot.Id,
            VehicleRegistration = "CAB-5678"
        };

        var result = await service.CreateAsync(
            userId,
            request,
            CancellationToken.None);

        Assert.Equal(
            ParkingReservationStatuses.Reserved,
            result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithCancelledBooking_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = new Booking
        {
            CustomerUserId = userId,
            EventId = eventId,
            Status = BookingStatus.Cancelled
        };

        var slot = new ParkingSlot
        {
            ParkingZoneId = Guid.NewGuid(),
            EventId = eventId,
            SlotCode = "A-03",
            Status = "Available"
        };

        var service = new ParkingReservationService(
            new FakeParkingReservationRepository(slot),
            new FakeBookingRepository(booking),
            new FakeSavedVehicleRepository());

        var request = new CreateParkingReservationRequest
        {
            BookingId = booking.Id,
            ParkingSlotId = slot.Id,
            VehicleRegistration = "CAB-9999"
        };

        var exception =
            await Assert.ThrowsAsync<ParkingReservationValidationException>(
                () => service.CreateAsync(
                    userId,
                    request,
                    CancellationToken.None));

        Assert.Equal(
            "Parking can only be reserved for a pending or confirmed booking.",
            exception.Message);
    }

    private sealed class FakeParkingReservationRepository(
        ParkingSlot slot)
        : IParkingReservationRepository
    {
        public ParkingReservation? AddedReservation { get; private set; }

        public Task<ParkingReservation?> GetByIdAsync(
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ParkingReservation?>(null);
        }

        public Task<ParkingReservation?> GetByBookingIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ParkingReservation?>(null);
        }

        public Task<ParkingReservation?> GetActiveByParkingSlotIdAsync(
            Guid parkingSlotId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ParkingReservation?>(null);
        }

        public Task<ParkingSlot?> GetParkingSlotByIdAsync(
            Guid parkingSlotId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ParkingSlot?>(
                parkingSlotId == slot.Id
                    ? slot
                    : null);
        }

        public Task AddAsync(
            ParkingReservation reservation,
            CancellationToken cancellationToken = default)
        {
            AddedReservation = reservation;
            return Task.CompletedTask;
        }

        public void Update(
            ParkingReservation reservation)
        {
        }

        public void UpdateParkingSlot(
            ParkingSlot parkingSlot)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeBookingRepository(
        Booking booking)
        : IBookingRepository
    {
        public Task<Booking?> GetByIdAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Booking?>(
                bookingId == booking.Id
                    ? booking
                    : null);
        }

        public Task<IReadOnlyList<Booking>> GetByCustomerAsync(
            Guid customerUserId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Booking> bookings = [];
            return Task.FromResult(bookings);
        }

        public Task<IReadOnlyList<Guid>> GetSeatIdsAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Guid> seatIds = [];
            return Task.FromResult(seatIds);
        }

        public Task AddAsync(
            Booking booking,
            IReadOnlyCollection<BookingSeat> bookingSeats,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSavedVehicleRepository
        : ISavedVehicleRepository
    {
        public Task<IReadOnlyList<SavedVehicle>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SavedVehicle> vehicles = [];
            return Task.FromResult(vehicles);
        }

        public Task<SavedVehicle?> GetByIdAsync(
            Guid vehicleId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SavedVehicle?>(null);
        }

        public Task AddAsync(
            SavedVehicle vehicle,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Update(
            SavedVehicle vehicle)
        {
        }

        public void Remove(
            SavedVehicle vehicle)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}