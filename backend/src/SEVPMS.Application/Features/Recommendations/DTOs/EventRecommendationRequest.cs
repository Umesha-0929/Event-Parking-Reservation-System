namespace SEVPMS.Application.Features.Recommendations.DTOs;

public sealed class EventRecommendationRequest
{
    public List<string> PreferredCategories { get; set; }
        = new();

    public int Limit { get; set; } = 10;
}