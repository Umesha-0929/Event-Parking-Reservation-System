namespace SEVPMS.Application.Features.Recommendations.DTOs;

public sealed class EventRecommendationDto
{
    public Guid EventId { get; set; }

    public Guid VenueId { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Description { get; set; }
        = string.Empty;

    public string Category { get; set; }
        = string.Empty;

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public int RecommendationScore { get; set; }

    public List<string> Reasons { get; set; }
        = new();
}