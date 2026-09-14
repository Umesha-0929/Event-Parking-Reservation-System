namespace SEVPMS.Application.Features.Reviews.DTOs;

public sealed class EventReviewDto
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Guid CustomerUserId { get; set; }

    public Guid BookingId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
