namespace SEVPMS.Application.Features.Reviews.DTOs;

public sealed class EventRatingSummaryDto
{
    public Guid EventId { get; set; }

    public int ReviewCount { get; set; }

    public double AverageRating { get; set; }
}