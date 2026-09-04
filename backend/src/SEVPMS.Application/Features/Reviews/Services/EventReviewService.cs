using SEVPMS.Application.Features.Reviews.DTOs;
using SEVPMS.Application.Features.Reviews.Interfaces;
using SEVPMS.Application.Features.Reviews.Validators;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Reviews;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Reviews.Services;

public sealed class EventReviewService(
    IEventReviewRepository reviewRepository,
    IBookingRepository bookingRepository,
    ITicketRepository ticketRepository)
    : IEventReviewService
{
    public async Task<EventReviewDto> CreateAsync(
        Guid customerUserId,
        Guid eventId,
        CreateEventReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        EventReviewValidator.ValidateCustomerId(
            customerUserId);

        EventReviewValidator.ValidateEventId(
            eventId);

        EventReviewValidator.ValidateCreateRequest(
            request);

        var booking =
            await bookingRepository.GetByIdAsync(
                request.BookingId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Booking was not found.");

        if (booking.CustomerUserId !=
            customerUserId)
        {
            throw new InvalidOperationException(
                "You can only review an event from your own booking.");
        }

        if (booking.EventId !=
            eventId)
        {
            throw new InvalidOperationException(
                "The booking does not belong to this event.");
        }

        var tickets =
            await ticketRepository.GetByBookingAsync(
                booking.Id,
                cancellationToken);

        var verifiedAttendee =
            tickets.Any(
                ticket =>
                    ticket.EventId == eventId &&
                    ticket.CheckedInAtUtc.HasValue);

        var eligibleCompletedBooking =
            booking.Status ==
            BookingStatus.Completed;

        if (!verifiedAttendee &&
            !eligibleCompletedBooking)
        {
            throw new InvalidOperationException(
                "Only verified attendees or customers with completed bookings can review this event.");
        }

        var existingReview =
            await reviewRepository
                .GetByEventAndCustomerAsync(
                    eventId,
                    customerUserId,
                    cancellationToken);

        if (existingReview is not null)
        {
            throw new InvalidOperationException(
                "You have already reviewed this event.");
        }

        var review =
            new EventReview
            {
                EventId =
                    eventId,
                CustomerUserId =
                    customerUserId,
                BookingId =
                    booking.Id,
                Rating =
                    request.Rating,
                Comment =
                    string.IsNullOrWhiteSpace(
                        request.Comment)
                        ? null
                        : request.Comment.Trim()
            };

        await reviewRepository.AddAsync(
            review,
            cancellationToken);

        await reviewRepository.SaveChangesAsync(
            cancellationToken);

        return Map(review);
    }

    public async Task<IReadOnlyList<EventReviewDto>>
        GetByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
    {
        EventReviewValidator.ValidateEventId(
            eventId);

        var reviews =
            await reviewRepository.GetByEventAsync(
                eventId,
                cancellationToken);

        return reviews
            .OrderByDescending(
                review => review.CreatedAtUtc)
            .Select(Map)
            .ToList();
    }

    public async Task<EventRatingSummaryDto>
        GetSummaryAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
    {
        EventReviewValidator.ValidateEventId(
            eventId);

        var reviews =
            await reviewRepository.GetByEventAsync(
                eventId,
                cancellationToken);

        if (reviews.Count == 0)
        {
            return new EventRatingSummaryDto
            {
                EventId =
                    eventId,
                ReviewCount =
                    0,
                AverageRating =
                    0
            };
        }

        return new EventRatingSummaryDto
        {
            EventId =
                eventId,
            ReviewCount =
                reviews.Count,
            AverageRating =
                Math.Round(
                    reviews.Average(
                        review => review.Rating),
                    2)
        };
    }

    private static EventReviewDto Map(
        EventReview review)
        => new()
        {
            Id =
                review.Id,
            EventId =
                review.EventId,
            CustomerUserId =
                review.CustomerUserId,
            BookingId =
                review.BookingId,
            Rating =
                review.Rating,
            Comment =
                review.Comment,
            CreatedAtUtc =
                review.CreatedAtUtc
        };
}