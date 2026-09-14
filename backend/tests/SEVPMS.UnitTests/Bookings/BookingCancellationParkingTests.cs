using SEVPMS.Application.Features.Bookings.Services;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Bookings;

public sealed class BookingCancellationParkingTests
{
    [Fact]
    public async Task CancelAsync_WithParkingReservation_ReleasesParkingAndSeatHold()
    {
        var userId = Guid.NewGuid();

        var booking = new Booking
        {
            CustomerUserId = userId,
            EventId = Guid.NewGuid(),
            BookingNumber = "BKG-001",
            HoldToken = "hold-001",
            Status = BookingStatus.Pending
        };

        var bookingRepository =
            new FakeBookingRepository(booking);

        var seatService =
            new FakeSeatService();

        var parkingService =
            new FakeParkingReservationService();

        var service = new BookingService(
            bookingRepository,
            new FakeEventRepository(),
            new FakeSeatingLayoutRepository(),
            seatService,
            parkingService);

        var result = await service.CancelAsync(
            userId,
            booking.Id,
            CancellationToken.None);

        Assert.Equal(
            BookingStatus.Cancelled,
            result.Status);

        Assert.True(
            parkingService.CancelByBookingCalled);

        Assert.Equal(
            booking.Id,
            parkingService.CancelledBookingId);

        Assert.True(
            seatService.ReleaseHoldCalled);

        Assert.Equal(
            booking.HoldToken,
            seatService.ReleasedHoldToken);

        Assert.True(
            bookingRepository.SaveChangesCalled);
    }

    private sealed class FakeBookingRepository(
        Booking booking)
        : IBookingRepository
    {
        public bool SaveChangesCalled { get; private set; }

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
            IReadOnlyList<Booking> bookings = [booking];
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
            Booking newBooking,
            IReadOnlyCollection<BookingSeat> bookingSeats,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeParkingReservationService
        : IParkingReservationService
    {
        public bool CancelByBookingCalled { get; private set; }

        public Guid CancelledBookingId { get; private set; }

        public Task<bool> CancelByBookingAsync(
            Guid userId,
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            CancelByBookingCalled = true;
            CancelledBookingId = bookingId;

            return Task.FromResult(true);
        }

        public Task<ParkingReservationDto?> GetByIdAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> CreateAsync(
            Guid userId,
            CreateParkingReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> MarkEnteredAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> MarkParkedAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> MarkExitedAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> CancelAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> ScanAsync(
            Guid userId,
            ParkingQrScanRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeSeatService
        : ISeatService
    {
        public bool ReleaseHoldCalled { get; private set; }

        public string? ReleasedHoldToken { get; private set; }

        public Task<bool> ReleaseHoldAsync(
            string holdToken,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            ReleaseHoldCalled = true;
            ReleasedHoldToken = holdToken;

            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<SeatAvailabilityDto>> GetAvailabilityAsync(
            Guid eventId,
            Guid? sectionId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatHoldResponse> HoldAsync(
            Guid eventId,
            Guid userId,
            CreateSeatHoldRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> CommitHoldAsync(
            string holdToken,
            Guid userId,
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatAvailabilityDto> UpsertSeatAsync(
            Guid eventId,
            UpsertSeatRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatViewAssetDto?> GetSeatViewAsync(
            Guid eventId,
            Guid seatId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatViewAssetDto> UpsertSeatViewAsync(
            Guid eventId,
            UpsertSeatViewAssetRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeEventRepository
        : IEventRepository
    {
        public Task<IReadOnlyList<Event>> GetPublishedAsync(
            EventSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Event>> GetByOrganizerUserIdAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task AddAsync(
            Event eventEntity,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeSeatingLayoutRepository
        : ISeatingLayoutRepository
    {
        public Task<SeatingLayout?> GetLayoutByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatingLayout?> GetPublishedLayoutByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<SeatSection>> GetSectionsAsync(
            Guid seatingLayoutId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<SeatCategory>> GetCategoriesAsync(
            Guid seatingLayoutId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<Seat>> GetSeatsAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatingLayout> AddLayoutAsync(
            SeatingLayout layout,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task UpdateLayoutAsync(
            SeatingLayout layout,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatSection> UpsertSectionAsync(
            SeatSection section,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<SeatCategory> UpsertCategoryAsync(
            SeatCategory category,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task ReplaceSectionSeatsAsync(
            Guid eventId,
            Guid sectionId,
            IReadOnlyCollection<Seat> seats,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}