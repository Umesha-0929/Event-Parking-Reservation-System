namespace SEVPMS.Application.Features.Reviews.DTOs;

public sealed class CreateEventReviewRequest
{
    public Guid BookingId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}
