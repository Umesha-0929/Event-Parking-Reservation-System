using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Bookings.DTOs;
using SEVPMS.Application.Features.Bookings.Interfaces;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Bookings.Services;

public sealed class BookingService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    ISeatingLayoutRepository seatingLayoutRepository,
    ISeatService seatService)
    : IBookingService
{
    public async Task<IReadOnlyList<BookingResponse>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default)
    {
        var bookings = await bookingRepository.GetByCustomerAsync(customerUserId, cancellationToken);
        var responses = new List<BookingResponse>(bookings.Count);

        foreach (var booking in bookings)
        {
            var seatIds = await bookingRepository.GetSeatIdsAsync(booking.Id, cancellationToken);
            responses.Add(Map(booking, seatIds));
        }

        return responses;
    }

    public async Task<BookingResponse> GetByIdAsync(Guid customerUserId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await GetOwnedAsync(customerUserId, bookingId, cancellationToken);
        var seatIds = await bookingRepository.GetSeatIdsAsync(booking.Id, cancellationToken);
        return Map(booking, seatIds);
    }

    public async Task<BookingResponse> CreateAsync(Guid customerUserId, CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EventId == Guid.Empty)
            throw new ArgumentException("Event is required.");
        if (string.IsNullOrWhiteSpace(request.HoldToken))
            throw new ArgumentException("Seat hold token is required.");

        var selectedIds = request.SeatIds?.Where(x => x != Guid.Empty).Distinct().ToArray()
            ?? Array.Empty<Guid>();

        if (selectedIds.Length == 0)
            throw new ArgumentException("At least one seat is required.");

        var eventEntity = await eventRepository.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event was not found.");

        if (eventEntity.Status != EventStatus.Published)
            throw new InvalidOperationException("Only published events can be booked.");

        var layout = await seatingLayoutRepository.GetPublishedLayoutByEventAsync(request.EventId, cancellationToken)
            ?? throw new InvalidOperationException("The event does not have a published seating layout.");

        var seats = await seatingLayoutRepository.GetSeatsAsync(request.EventId, cancellationToken);
        var categories = await seatingLayoutRepository.GetCategoriesAsync(layout.Id, cancellationToken);
        var seatMap = seats.ToDictionary(x => x.Id);
        var categoryMap = categories.ToDictionary(x => x.Id);

        decimal total = 0m;
        foreach (var seatId in selectedIds)
        {
            if (!seatMap.TryGetValue(seatId, out var seat))
                throw new ArgumentException($"Seat {seatId} does not belong to this event.");

            if (seat.Status is SeatStatus.Booked or SeatStatus.Blocked)
                throw new InvalidOperationException($"Seat {seat.SeatNumber} is not available.");

            if (seat.SeatCategoryId is null ||
                !categoryMap.TryGetValue(seat.SeatCategoryId.Value, out var category) ||
                !category.IsActive)
            {
                throw new InvalidOperationException($"Seat {seat.SeatNumber} does not have an active price category.");
            }

            total += category.Price;
        }

        var booking = new Booking
        {
            BookingNumber = $"BKG-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            CustomerUserId = customerUserId,
            EventId = request.EventId,
            HoldToken = request.HoldToken.Trim(),
            TotalAmount = total,
            Status = BookingStatus.Pending
        };

        var bookingSeats = selectedIds
            .Select(id => new BookingSeat { BookingId = booking.Id, SeatId = id })
            .ToArray();

        await bookingRepository.AddAsync(booking, bookingSeats, cancellationToken);
        await bookingRepository.SaveChangesAsync(cancellationToken);

        return Map(booking, selectedIds);
    }

    public async Task<BookingResponse> CancelAsync(Guid customerUserId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await GetOwnedAsync(customerUserId, bookingId, cancellationToken);

        if (booking.Status == BookingStatus.Confirmed)
            throw new InvalidOperationException("Confirmed bookings cannot be cancelled through this endpoint.");

        if (booking.Status == BookingStatus.Cancelled)
        {
            var existingSeatIds = await bookingRepository.GetSeatIdsAsync(booking.Id, cancellationToken);
            return Map(booking, existingSeatIds);
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;
        booking.UpdatedAtUtc = DateTime.UtcNow;

        await seatService.ReleaseHoldAsync(booking.HoldToken, customerUserId, cancellationToken);
        await bookingRepository.SaveChangesAsync(cancellationToken);

        var seatIds = await bookingRepository.GetSeatIdsAsync(booking.Id, cancellationToken);
        return Map(booking, seatIds);
    }

    private async Task<Booking> GetOwnedAsync(Guid customerUserId, Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking was not found.");

        if (booking.CustomerUserId != customerUserId)
            throw new ForbiddenAccessException("You do not own this booking.");

        return booking;
    }

    private static BookingResponse Map(Booking x, IReadOnlyList<Guid> seatIds) => new()
    {
        BookingId = x.Id,
        BookingNumber = x.BookingNumber,
        CustomerUserId = x.CustomerUserId,
        EventId = x.EventId,
        SeatIds = seatIds,
        TotalAmount = x.TotalAmount,
        Status = x.Status,
        CreatedAtUtc = x.CreatedAtUtc,
        ConfirmedAtUtc = x.ConfirmedAtUtc,
        CancelledAtUtc = x.CancelledAtUtc
    };
}
