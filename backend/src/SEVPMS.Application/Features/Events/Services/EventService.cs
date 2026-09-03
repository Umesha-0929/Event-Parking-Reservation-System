using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Events.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Events.Services;

public sealed class EventService(
    IEventRepository eventRepository,
    IVenueRepository venueRepository,
    IVenueRentalRepository venueRentalRepository,
    IEventCategoryRepository? eventCategoryRepository = null)
    : IEventService
{
    public async Task<IReadOnlyList<EventResponse>> GetPublishedAsync(
        EventSearchRequest request,
        CancellationToken cancellationToken = default)
        => (await eventRepository.GetPublishedAsync(request, cancellationToken)).Select(Map).ToList();

    public async Task<EventResponse> GetPublicByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var entity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (entity is null || entity.Status != EventStatus.Published)
            throw new KeyNotFoundException("Event was not found.");
        return Map(entity);
    }

    public async Task<IReadOnlyList<EventResponse>> GetMineAsync(
        Guid organizerUserId,
        CancellationToken cancellationToken = default)
        => (await eventRepository.GetByOrganizerUserIdAsync(organizerUserId, cancellationToken)).Select(Map).ToList();

    public async Task<EventResponse> CreateAsync(
        Guid organizerUserId,
        CreateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var canonicalCategory = await ValidateRequestAsync(
            request.VenueId, request.Title, request.Category,
            request.StartAtUtc, request.EndAtUtc, cancellationToken);

        var entity = new Event
        {
            OrganizerUserId = organizerUserId,
            VenueId = request.VenueId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Category = canonicalCategory,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = request.EndAtUtc,
            Status = EventStatus.Draft
        };

        await eventRepository.AddAsync(entity, cancellationToken);
        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<EventResponse> UpdateAsync(
        Guid organizerUserId,
        Guid eventId,
        UpdateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOwnedAsync(organizerUserId, eventId, cancellationToken);

        if (entity.Status is EventStatus.Completed or EventStatus.Cancelled)
            throw new InvalidOperationException("Completed or cancelled events cannot be edited.");

        var canonicalCategory = await ValidateRequestAsync(
            request.VenueId, request.Title, request.Category,
            request.StartAtUtc, request.EndAtUtc, cancellationToken);

        entity.VenueId = request.VenueId;
        entity.Title = request.Title.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.Category = canonicalCategory;
        entity.StartAtUtc = request.StartAtUtc;
        entity.EndAtUtc = request.EndAtUtc;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<EventResponse> PublishAsync(
        Guid organizerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOwnedAsync(organizerUserId, eventId, cancellationToken);

        if (entity.Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published.");

        var venue = await venueRepository.GetByIdAsync(entity.VenueId, cancellationToken);
        if (venue is null || !venue.IsActive)
            throw new InvalidOperationException("The selected venue is not active.");

        var accepted = await venueRentalRepository.HasAcceptedRentalForOrganizerAsync(
            organizerUserId,
            entity.VenueId,
            entity.StartAtUtc,
            entity.EndAtUtc,
            cancellationToken);

        if (!accepted)
            throw new InvalidOperationException(
                "An accepted venue rental covering the event time is required before publishing.");

        if (eventCategoryRepository is not null &&
            await eventCategoryRepository.FindActiveAsync(entity.Category, cancellationToken) is null)
        {
            throw new InvalidOperationException("The event category is no longer active.");
        }

        entity.Status = EventStatus.Published;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<EventResponse> CancelAsync(
        Guid organizerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetOwnedAsync(organizerUserId, eventId, cancellationToken);

        if (entity.Status == EventStatus.Completed)
            throw new InvalidOperationException("Completed events cannot be cancelled.");

        entity.Status = EventStatus.Cancelled;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    private async Task<Event> GetOwnedAsync(
        Guid organizerUserId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var entity = await eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event was not found.");

        if (entity.OrganizerUserId != organizerUserId)
            throw new ForbiddenAccessException("You do not have permission to manage this event.");

        return entity;
    }

    private async Task<string> ValidateRequestAsync(
        Guid venueId,
        string title,
        string category,
        DateTime startAtUtc,
        DateTime endAtUtc,
        CancellationToken cancellationToken)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Venue is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Event title is required.");
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Event category is required.");
        if (startAtUtc == default || endAtUtc == default || endAtUtc <= startAtUtc)
            throw new ArgumentException("Event end time must be later than start time.");

        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken);
        if (venue is null || !venue.IsActive)
            throw new ArgumentException("The selected venue does not exist or is inactive.");

        if (eventCategoryRepository is null)
            return category.Trim();

        var master = await eventCategoryRepository.FindActiveAsync(category, cancellationToken);
        if (master is null)
            throw new ArgumentException("Event category must match an active category master record.");

        return master.Name;
    }

    private static EventResponse Map(Event x) => new()
    {
        EventId = x.Id,
        OrganizerUserId = x.OrganizerUserId,
        VenueId = x.VenueId,
        Title = x.Title,
        Description = x.Description,
        Category = x.Category,
        StartAtUtc = x.StartAtUtc,
        EndAtUtc = x.EndAtUtc,
        Status = x.Status,
        CreatedAtUtc = x.CreatedAtUtc,
        UpdatedAtUtc = x.UpdatedAtUtc
    };
}
