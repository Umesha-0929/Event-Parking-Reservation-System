using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Events.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Events.Services;

public sealed class EventService(
    IEventRepository eventRepository,
    IVenueRepository venueRepository)
    : IEventService
{
    public async Task<IReadOnlyList<EventResponse>> GetPublishedAsync(CancellationToken cancellationToken = default)
        => (await eventRepository.GetPublishedAsync(cancellationToken)).Select(Map).ToList();

    public async Task<EventResponse> GetPublicByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity is null || eventEntity.Status != EventStatus.Published)
            throw new KeyNotFoundException("Event was not found.");
        return Map(eventEntity);
    }

    public async Task<IReadOnlyList<EventResponse>> GetMineAsync(Guid organizerUserId, CancellationToken cancellationToken = default)
        => (await eventRepository.GetByOrganizerUserIdAsync(organizerUserId, cancellationToken)).Select(Map).ToList();

    public async Task<EventResponse> CreateAsync(Guid organizerUserId, CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request.VenueId, request.Title, request.Category, request.StartAtUtc, request.EndAtUtc, cancellationToken);

        var eventEntity = new Event
        {
            OrganizerUserId = organizerUserId,
            VenueId = request.VenueId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Category = request.Category.Trim(),
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = request.EndAtUtc,
            Status = EventStatus.Draft
        };

        await eventRepository.AddAsync(eventEntity, cancellationToken);
        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(eventEntity);
    }

    public async Task<EventResponse> UpdateAsync(Guid organizerUserId, Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken = default)
    {
        var eventEntity = await GetOwnedAsync(organizerUserId, eventId, cancellationToken);

        if (eventEntity.Status is EventStatus.Completed or EventStatus.Cancelled)
            throw new InvalidOperationException("Completed or cancelled events cannot be edited.");

        await ValidateRequestAsync(request.VenueId, request.Title, request.Category, request.StartAtUtc, request.EndAtUtc, cancellationToken);

        eventEntity.VenueId = request.VenueId;
        eventEntity.Title = request.Title.Trim();
        eventEntity.Description = request.Description?.Trim() ?? string.Empty;
        eventEntity.Category = request.Category.Trim();
        eventEntity.StartAtUtc = request.StartAtUtc;
        eventEntity.EndAtUtc = request.EndAtUtc;
        eventEntity.UpdatedAtUtc = DateTime.UtcNow;

        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(eventEntity);
    }

    public async Task<EventResponse> PublishAsync(Guid organizerUserId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await GetOwnedAsync(organizerUserId, eventId, cancellationToken);

        if (eventEntity.Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published.");

        var venue = await venueRepository.GetByIdAsync(eventEntity.VenueId, cancellationToken);
        if (venue is null || !venue.IsActive)
            throw new InvalidOperationException("The selected venue is not active.");

        eventEntity.Status = EventStatus.Published;
        eventEntity.UpdatedAtUtc = DateTime.UtcNow;
        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(eventEntity);
    }

    public async Task<EventResponse> CancelAsync(Guid organizerUserId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await GetOwnedAsync(organizerUserId, eventId, cancellationToken);
        if (eventEntity.Status == EventStatus.Completed)
            throw new InvalidOperationException("Completed events cannot be cancelled.");

        eventEntity.Status = EventStatus.Cancelled;
        eventEntity.UpdatedAtUtc = DateTime.UtcNow;
        await eventRepository.SaveChangesAsync(cancellationToken);
        return Map(eventEntity);
    }

    private async Task<Event> GetOwnedAsync(Guid organizerUserId, Guid eventId, CancellationToken cancellationToken)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("Event was not found.");

        if (eventEntity.OrganizerUserId != organizerUserId)
            throw new ForbiddenAccessException("You do not have permission to manage this event.");

        return eventEntity;
    }

    private async Task ValidateRequestAsync(
        Guid venueId,
        string title,
        string category,
        DateTime startAtUtc,
        DateTime endAtUtc,
        CancellationToken cancellationToken)
    {
        if (venueId == Guid.Empty)
            throw new ArgumentException("Venue is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Event title is required.");
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Event category is required.");
        if (startAtUtc == default || endAtUtc == default || endAtUtc <= startAtUtc)
            throw new ArgumentException("Event end time must be later than start time.");

        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken);
        if (venue is null || !venue.IsActive)
            throw new ArgumentException("The selected venue does not exist or is inactive.");
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
