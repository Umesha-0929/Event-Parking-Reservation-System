using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Recommendations.DTOs;
using SEVPMS.Application.Features.Recommendations.Interfaces;
using SEVPMS.Application.Features.Recommendations.Validators;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Recommendations.Services;

public sealed class EventRecommendationService(
    IEventRepository eventRepository,
    IBookingRepository bookingRepository)
    : IEventRecommendationService
{
    public async Task<IReadOnlyList<EventRecommendationDto>>
        GetRecommendationsAsync(
            Guid customerUserId,
            EventRecommendationRequest request,
            CancellationToken cancellationToken = default)
    {
        EventRecommendationValidator.ValidateCustomerId(
            customerUserId);

        EventRecommendationValidator.ValidateRequest(
            request);

        var nowUtc =
            DateTime.UtcNow;

        var bookings =
            await bookingRepository.GetByCustomerAsync(
                customerUserId,
                cancellationToken);

        var activeBookedEventIds =
            bookings
                .Where(booking =>
                    booking.Status !=
                    BookingStatus.Cancelled)
                .Select(booking =>
                    booking.EventId)
                .ToHashSet();

        var historyBookings =
            bookings
                .Where(booking =>
                    booking.Status ==
                        BookingStatus.Confirmed ||
                    booking.Status ==
                        BookingStatus.Completed)
                .ToList();

        var historyCategoryCounts =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var booking in historyBookings)
        {
            var historicalEvent =
                await eventRepository.GetByIdAsync(
                    booking.EventId,
                    cancellationToken);

            if (historicalEvent is null ||
                string.IsNullOrWhiteSpace(
                    historicalEvent.Category))
            {
                continue;
            }

            var category =
                historicalEvent.Category.Trim();

            historyCategoryCounts[category] =
                historyCategoryCounts.TryGetValue(
                    category,
                    out var count)
                    ? count + 1
                    : 1;
        }

        var preferredCategories =
            request.PreferredCategories
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        var publishedEvents =
            await eventRepository.GetPublishedAsync(
                new EventSearchRequest(),
                cancellationToken);

        return publishedEvents
            .Where(eventEntity =>
                eventEntity.Status ==
                    EventStatus.Published &&
                eventEntity.StartAtUtc >
                    nowUtc &&
                !activeBookedEventIds.Contains(
                    eventEntity.Id))
            .Select(eventEntity =>
                BuildRecommendation(
                    eventEntity,
                    preferredCategories,
                    historyCategoryCounts))
            .OrderByDescending(result =>
                result.RecommendationScore)
            .ThenBy(result =>
                result.StartAtUtc)
            .Take(request.Limit)
            .ToList();
    }

    private static EventRecommendationDto
        BuildRecommendation(
            Event eventEntity,
            HashSet<string> preferredCategories,
            IReadOnlyDictionary<string, int>
                historyCategoryCounts)
    {
        var score = 0;
        var reasons =
            new List<string>();

        var category =
            eventEntity.Category.Trim();

        if (preferredCategories.Contains(
                category))
        {
            score += 5;

            reasons.Add(
                "Matches your preferred category.");
        }

        if (historyCategoryCounts.TryGetValue(
                category,
                out var historyCount))
        {
            score +=
                Math.Min(
                    historyCount * 2,
                    6);

            reasons.Add(
                "Similar to events you booked before.");
        }

        if (reasons.Count == 0)
        {
            reasons.Add(
                "Upcoming published event.");
        }

        return new EventRecommendationDto
        {
            EventId =
                eventEntity.Id,
            VenueId =
                eventEntity.VenueId,
            Title =
                eventEntity.Title,
            Description =
                eventEntity.Description,
            Category =
                eventEntity.Category,
            StartAtUtc =
                eventEntity.StartAtUtc,
            EndAtUtc =
                eventEntity.EndAtUtc,
            RecommendationScore =
                score,
            Reasons =
                reasons
        };
    }
}