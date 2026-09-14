using SEVPMS.Application.Features.Recommendations.DTOs;

namespace SEVPMS.Application.Features.Recommendations.Validators;

public static class EventRecommendationValidator
{
    public static void ValidateCustomerId(
        Guid customerUserId)
    {
        if (customerUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer is required.");
        }
    }

    public static void ValidateRequest(
        EventRecommendationRequest request)
    {
        if (request.Limit < 1 ||
            request.Limit > 50)
        {
            throw new ArgumentException(
                "Recommendation limit must be between 1 and 50.");
        }

        request.PreferredCategories =
            request.PreferredCategories
                .Where(category =>
                    !string.IsNullOrWhiteSpace(category))
                .Select(category =>
                    category.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}