using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Events.DTOs;

public sealed class CreateEventRequest
{
    public Guid VenueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
}

public sealed class UpdateEventRequest
{
    public Guid VenueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
}

public sealed class EventResponse
{
    public Guid EventId { get; set; }
    public Guid OrganizerUserId { get; set; }
    public Guid VenueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public EventStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class EventSearchRequest
{
    public string? Search { get; set; }

    public Guid? Venue { get; set; }

    public string? Category { get; set; }

    public DateOnly? Date { get; set; }
}
