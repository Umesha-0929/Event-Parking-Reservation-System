using SEVPMS.Application.Features.Reviews.DTOs;

namespace SEVPMS.Application.Features.Reviews.Validators;

public static class EventReviewValidator
{
    public static void ValidateEventId(
        Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Event is required.");
        }
    }

    public static void ValidateCustomerId(
        Guid customerUserId)
    {
        if (customerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer is required.");
        }
    }

    public static void ValidateCreateRequest(
        CreateEventReviewRequest request)
    {
        if (request.BookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Booking is required.");
        }

        if (request.Rating < 1 ||
            request.Rating > 5)
        {
            throw new ArgumentException(
                "Rating must be between 1 and 5.");
        }

        if (!string.IsNullOrWhiteSpace(
                request.Comment) &&
            request.Comment.Trim().Length > 1000)
        {
            throw new ArgumentException(
                "Review comment cannot exceed 1000 characters.");
        }
    }
}