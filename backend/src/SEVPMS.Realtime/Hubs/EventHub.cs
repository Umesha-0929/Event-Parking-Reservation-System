using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Realtime.Groups;

namespace SEVPMS.Realtime.Hubs;

[Authorize]
public sealed class EventHub(
    IEventRepository eventRepository) : Hub
{
    public async Task JoinEvent(
        Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new HubException(
                "A valid event is required.");
        }

        var eventEntity =
            await eventRepository.GetByIdAsync(eventId);

        if (eventEntity is null)
        {
            throw new HubException(
                "Event was not found.");
        }

        var isAdmin =
            Context.User?.IsInRole("Admin") == true;

        var isOrganizer =
            Context.User?.IsInRole(
                "EventOrganizer") == true;

        var userId =
            GetCurrentUserId();

        var ownsEvent =
            isOrganizer &&
            userId.HasValue &&
            eventEntity.OrganizerUserId ==
                userId.Value;

        var isPublished =
            string.Equals(
                eventEntity.Status.ToString(),
                "Published",
                StringComparison.OrdinalIgnoreCase);

        if (!isPublished &&
            !isAdmin &&
            !ownsEvent)
        {
            throw new HubException(
                "You cannot subscribe to this event.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            RealtimeGroupNames.Event(eventId));
    }

    public async Task LeaveEvent(
        Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            RealtimeGroupNames.Event(eventId));
    }

    public async Task JoinEventStaff(
        Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            throw new HubException(
                "A valid event is required.");
        }

        var eventEntity =
            await eventRepository.GetByIdAsync(eventId);

        if (eventEntity is null)
        {
            throw new HubException(
                "Event was not found.");
        }

        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                RealtimeGroupNames.EventStaff(eventId));

            return;
        }

        if (Context.User?.IsInRole(
                "EventOrganizer") != true)
        {
            throw new HubException(
                "Staff access is required.");
        }

        var userId =
            GetCurrentUserId();

        if (!userId.HasValue ||
            eventEntity.OrganizerUserId !=
                userId.Value)
        {
            throw new HubException(
                "You do not manage this event.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            RealtimeGroupNames.EventStaff(eventId));
    }

    public async Task LeaveEventStaff(
        Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            RealtimeGroupNames.EventStaff(eventId));
    }

    private Guid? GetCurrentUserId()
    {
        var raw =
            Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        return Guid.TryParse(raw, out var userId)
            ? userId
            : null;
    }
}