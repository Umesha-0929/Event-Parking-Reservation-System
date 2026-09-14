using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Api.Klegar;

/// <summary>
/// Centralizes tenant/ownership checks for Klegar seat, ticket and check-in HTTP endpoints.
/// Business services remain unchanged; this service only protects the API boundary.
/// </summary>
public sealed class KlegarAuthorizationService(
    IEventRepository eventRepository,
    IBookingRepository bookingRepository,
    ITicketRepository ticketRepository,
    RequestUserResolver users)
{
    public async Task<bool> CanManageEventAsync(
        HttpContext context,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        if (!users.TryGetUserId(context, out var userId)) return false;
        if (context.User.IsInRole(UserRole.Admin.ToString())) return true;
        if (!context.User.IsInRole(UserRole.EventOrganizer.ToString())) return false;

        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        return eventEntity is not null && eventEntity.OrganizerUserId == userId;
    }

    public async Task<bool> CanAccessBookingAsync(
        HttpContext context,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        if (!users.TryGetUserId(context, out var userId)) return false;
        if (context.User.IsInRole(UserRole.Admin.ToString())) return true;

        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking is null) return false;

        if (context.User.IsInRole(UserRole.Customer.ToString()))
            return booking.CustomerUserId == userId;

        if (!context.User.IsInRole(UserRole.EventOrganizer.ToString())) return false;
        var eventEntity = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken);
        return eventEntity is not null && eventEntity.OrganizerUserId == userId;
    }

    public async Task<bool> CanManageBookingAsync(
        HttpContext context,
        Guid bookingId,
        Guid? expectedEventId = null,
        CancellationToken cancellationToken = default)
    {
        if (!users.TryGetUserId(context, out var userId)) return false;
        if (!context.User.IsInRole(UserRole.Admin.ToString()) &&
            !context.User.IsInRole(UserRole.EventOrganizer.ToString())) return false;

        var booking = await bookingRepository.GetByIdAsync(bookingId, cancellationToken);
        if (booking is null) return false;
        if (expectedEventId.HasValue && booking.EventId != expectedEventId.Value) return false;
        if (context.User.IsInRole(UserRole.Admin.ToString())) return true;

        var eventEntity = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken);
        return eventEntity is not null && eventEntity.OrganizerUserId == userId;
    }

    public async Task<bool> CanAccessTicketAsync(
        HttpContext context,
        string ticketNo,
        CancellationToken cancellationToken = default)
    {
        if (!users.TryGetUserId(context, out var userId)) return false;
        if (context.User.IsInRole(UserRole.Admin.ToString())) return true;

        var ticket = await ticketRepository.GetByTicketNoAsync(ticketNo.Trim(), cancellationToken);
        if (ticket is null) return false;

        if (context.User.IsInRole(UserRole.Customer.ToString()))
        {
            var booking = await bookingRepository.GetByIdAsync(ticket.BookingId, cancellationToken);
            return booking is not null && booking.CustomerUserId == userId;
        }

        if (!context.User.IsInRole(UserRole.EventOrganizer.ToString())) return false;
        var eventEntity = await eventRepository.GetByIdAsync(ticket.EventId, cancellationToken);
        return eventEntity is not null && eventEntity.OrganizerUserId == userId;
    }

    public async Task<bool> CanManageTicketAsync(
        HttpContext context,
        string ticketNo,
        CancellationToken cancellationToken = default)
    {
        if (!users.TryGetUserId(context, out var userId)) return false;
        if (!context.User.IsInRole(UserRole.Admin.ToString()) &&
            !context.User.IsInRole(UserRole.EventOrganizer.ToString())) return false;

        var ticket = await ticketRepository.GetByTicketNoAsync(ticketNo.Trim(), cancellationToken);
        if (ticket is null) return false;
        if (context.User.IsInRole(UserRole.Admin.ToString())) return true;

        var eventEntity = await eventRepository.GetByIdAsync(ticket.EventId, cancellationToken);
        return eventEntity is not null && eventEntity.OrganizerUserId == userId;
    }
}
