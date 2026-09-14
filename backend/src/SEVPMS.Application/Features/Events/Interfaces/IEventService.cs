using SEVPMS.Application.Features.Events.DTOs;

namespace SEVPMS.Application.Features.Events.Interfaces;

public interface IEventService
{
    Task<IReadOnlyList<EventResponse>> GetPublishedAsync(
        EventSearchRequest request,
        CancellationToken cancellationToken = default);    
    Task<EventResponse> GetPublicByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventResponse>> GetMineAsync(Guid organizerUserId, CancellationToken cancellationToken = default);
    Task<EventResponse> CreateAsync(Guid organizerUserId, CreateEventRequest request, CancellationToken cancellationToken = default);
    Task<EventResponse> UpdateAsync(Guid organizerUserId, Guid eventId, UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task<EventResponse> PublishAsync(Guid organizerUserId, Guid eventId, CancellationToken cancellationToken = default);
    Task<EventResponse> CancelAsync(Guid organizerUserId, Guid eventId, CancellationToken cancellationToken = default);
}
