using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Audit.Interfaces;
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
    IEventCategoryRepository? eventCategoryRepository = null,
    IAuditLogService? auditLogService = null)
    : IEventService
{
    public async Task<IReadOnlyList<EventResponse>> GetPublishedAsync(
        EventSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var events =
            await eventRepository.GetPublishedAsync(
                request,
                cancellationToken);

        return events
            .Select(Map)
            .ToList();
    }

    public async Task<EventResponse> GetPublicByIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await eventRepository.GetByIdAsync(
                eventId,
                cancellationToken);

        if (entity is null ||
            entity.Status != EventStatus.Published)
        {
            throw new KeyNotFoundException(
                "Event was not found.");
        }

        return Map(entity);
    }

    public async Task<IReadOnlyList<EventResponse>> GetMineAsync(
        Guid organizerUserId,
        CancellationToken cancellationToken = default)
    {
        var events =
            await eventRepository.GetByOrganizerUserIdAsync(
                organizerUserId,
                cancellationToken);

        return events
            .Select(Map)
            .ToList();
    }

    public async Task<EventResponse> CreateAsync(
        Guid organizerUserId,
        CreateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category =
            await ValidateRequestAsync(
                request.VenueId,
                request.Title,
                request.CategoryId,
                request.Category,
                request.StartAtUtc,
                request.EndAtUtc,
                cancellationToken);

        var entity =
            new Event
            {
                OrganizerUserId =
                    organizerUserId,

                VenueId =
                    request.VenueId,

                CategoryId =
                    category.Id,

                Category =
                    category.Name,

                Title =
                    request.Title.Trim(),

                Description =
                    request.Description?.Trim()
                    ?? string.Empty,

                StartAtUtc =
                    request.StartAtUtc,

                EndAtUtc =
                    request.EndAtUtc,

                Status =
                    EventStatus.Draft
            };

        await eventRepository.AddAsync(
            entity,
            cancellationToken);

        await eventRepository.SaveChangesAsync(
            cancellationToken);

        return Map(entity);
    }

    public async Task<EventResponse> UpdateAsync(
        Guid organizerUserId,
        Guid eventId,
        UpdateEventRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity =
            await GetOwnedAsync(
                organizerUserId,
                eventId,
                cancellationToken);

        if (entity.Status is
            EventStatus.Completed or
            EventStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Completed or cancelled events cannot be edited.");
        }

        var category =
            await ValidateRequestAsync(
                request.VenueId,
                request.Title,
                request.CategoryId,
                request.Category,
                request.StartAtUtc,
                request.EndAtUtc,
                cancellationToken);

        entity.VenueId =
            request.VenueId;

        entity.CategoryId =
            category.Id;

        entity.Category =
            category.Name;

        entity.CategoryEntity =
            null;

        entity.Title =
            request.Title.Trim();

        entity.Description =
            request.Description?.Trim()
            ?? string.Empty;

        entity.StartAtUtc =
            request.StartAtUtc;

        entity.EndAtUtc =
            request.EndAtUtc;

        entity.UpdatedAtUtc =
            DateTime.UtcNow;

        await eventRepository.SaveChangesAsync(
            cancellationToken);

        return Map(entity);
    }

    public async Task<EventResponse> PublishAsync(
        Guid organizerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await GetOwnedAsync(
                organizerUserId,
                eventId,
                cancellationToken);

        if (entity.Status != EventStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft events can be published.");
        }

        var venue =
            await venueRepository.GetByIdAsync(
                entity.VenueId,
                cancellationToken);

        if (venue is null ||
            !venue.IsActive)
        {
            throw new InvalidOperationException(
                "The selected venue is not active.");
        }

        var accepted =
            await venueRentalRepository
                .HasAcceptedRentalForOrganizerAsync(
                    organizerUserId,
                    entity.VenueId,
                    entity.StartAtUtc,
                    entity.EndAtUtc,
                    cancellationToken);

        if (!accepted)
        {
            throw new InvalidOperationException(
                "An accepted venue rental covering the event time is required before publishing.");
        }

        if (eventCategoryRepository is not null)
        {
            EventCategory? category = null;

            if (entity.CategoryId != Guid.Empty)
            {
                category =
                    await eventCategoryRepository
                        .GetByIdAsync(
                            entity.CategoryId,
                            cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(
                         entity.Category))
            {
                category =
                    await eventCategoryRepository
                        .FindActiveAsync(
                            entity.Category,
                            cancellationToken);
            }

            if (category is null ||
                !category.IsActive)
            {
                throw new InvalidOperationException(
                    "The event category is no longer active.");
            }

            entity.CategoryId =
                category.Id;

            entity.Category =
                category.Name;
        }

        var previousStatus =
            entity.Status;

        entity.Status =
            EventStatus.Published;

        entity.UpdatedAtUtc =
            DateTime.UtcNow;

        await eventRepository.SaveChangesAsync(
            cancellationToken);

        if (auditLogService is not null)
        {
            await auditLogService.WriteAsync(
                organizerUserId,
                "Event published",
                "Event",
                entity.Id.ToString(),
                $"Status={previousStatus}",
                $"Status={entity.Status}",
                null,
                null,
                cancellationToken);
        }

        return Map(entity);
    }

    public async Task<EventResponse> CancelAsync(
        Guid organizerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await GetOwnedAsync(
                organizerUserId,
                eventId,
                cancellationToken);

        if (entity.Status == EventStatus.Completed)
        {
            throw new InvalidOperationException(
                "Completed events cannot be cancelled.");
        }

        if (entity.Status == EventStatus.Cancelled)
        {
            return Map(entity);
        }

        var previousStatus =
            entity.Status;

        entity.Status =
            EventStatus.Cancelled;

        entity.UpdatedAtUtc =
            DateTime.UtcNow;

        await eventRepository.SaveChangesAsync(
            cancellationToken);

        if (auditLogService is not null)
        {
            await auditLogService.WriteAsync(
                organizerUserId,
                "Event cancelled",
                "Event",
                entity.Id.ToString(),
                $"Status={previousStatus}",
                $"Status={entity.Status}",
                null,
                null,
                cancellationToken);
        }

        return Map(entity);
    }

    private async Task<Event> GetOwnedAsync(
        Guid organizerUserId,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var entity =
            await eventRepository.GetByIdAsync(
                eventId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Event was not found.");

        if (entity.OrganizerUserId !=
            organizerUserId)
        {
            throw new ForbiddenAccessException(
                "You do not have permission to manage this event.");
        }

        return entity;
    }

    private async Task<EventCategory> ValidateRequestAsync(
        Guid venueId,
        string title,
        Guid categoryId,
        string legacyCategory,
        DateTime startAtUtc,
        DateTime endAtUtc,
        CancellationToken cancellationToken)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException(
                "Venue is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Event title is required.");
        }

        if (startAtUtc == default ||
            endAtUtc == default ||
            endAtUtc <= startAtUtc)
        {
            throw new ArgumentException(
                "Event end time must be later than start time.");
        }

        var venue =
            await venueRepository.GetByIdAsync(
                venueId,
                cancellationToken);

        if (venue is null ||
            !venue.IsActive)
        {
            throw new ArgumentException(
                "The selected venue does not exist or is inactive.");
        }

        if (eventCategoryRepository is null)
        {
            if (categoryId == Guid.Empty &&
                string.IsNullOrWhiteSpace(
                    legacyCategory))
            {
                throw new ArgumentException(
                    "Event category is required.");
            }

            return new EventCategory
            {
                Id =
                    categoryId == Guid.Empty
                        ? Guid.NewGuid()
                        : categoryId,

                Name =
                    string.IsNullOrWhiteSpace(
                        legacyCategory)
                        ? "Category"
                        : legacyCategory.Trim(),

                Code =
                    "COMPAT",

                IsActive =
                    true
            };
        }

        EventCategory? category;

        if (categoryId != Guid.Empty)
        {
            category =
                await eventCategoryRepository
                    .GetByIdAsync(
                        categoryId,
                        cancellationToken);

            if (category is null ||
                !category.IsActive)
            {
                throw new ArgumentException(
                    "Event category must reference an active category.");
            }

            return category;
        }

        if (string.IsNullOrWhiteSpace(
                legacyCategory))
        {
            throw new ArgumentException(
                "Event category is required.");
        }

        category =
            await eventCategoryRepository
                .FindActiveAsync(
                    legacyCategory.Trim(),
                    cancellationToken);

        if (category is null)
        {
            throw new ArgumentException(
                "Event category must match an active category master record.");
        }

        return category;
    }

    private static EventResponse Map(
        Event entity)
    {
        return new EventResponse
        {
            EventId =
                entity.Id,

            OrganizerUserId =
                entity.OrganizerUserId,

            VenueId =
                entity.VenueId,

            CategoryId =
                entity.CategoryId,

            Category =
                entity.CategoryEntity?.Name
                ?? entity.Category,

            Title =
                entity.Title,

            Description =
                entity.Description,

            StartAtUtc =
                entity.StartAtUtc,

            EndAtUtc =
                entity.EndAtUtc,

            Status =
                entity.Status,

            CreatedAtUtc =
                entity.CreatedAtUtc,

            UpdatedAtUtc =
                entity.UpdatedAtUtc
        };
    }
}