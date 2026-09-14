using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SEVPMS.Api.Klegar;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Bookings;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.IntegrationTests.Klegar;

public sealed class KlegarAuthorizationContractTests
{
    [Fact]
    public async Task Organizer_cannot_manage_another_organizers_event()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var sut = Create(
            events: new Event { Id = eventId, OrganizerUserId = ownerId, VenueId = Guid.NewGuid(), CategoryId = Guid.NewGuid() });

        var allowed = await sut.CanManageEventAsync(Context(attackerId, UserRole.EventOrganizer), eventId);

        Assert.False(allowed);
    }

    [Fact]
    public async Task Customer_cannot_read_another_customers_booking_tickets()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var sut = Create(
            bookings: new Booking { Id = bookingId, CustomerUserId = ownerId, EventId = eventId, BookingNumber = "B1", HoldToken = "h" });

        var allowed = await sut.CanAccessBookingAsync(Context(attackerId, UserRole.Customer), bookingId);

        Assert.False(allowed);
    }

    [Fact]
    public async Task Organizer_cannot_manage_another_organizers_ticket()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var ticketNo = "T-001";
        var sut = Create(
            events: new Event { Id = eventId, OrganizerUserId = ownerId, VenueId = Guid.NewGuid(), CategoryId = Guid.NewGuid() },
            bookings: new Booking { Id = bookingId, CustomerUserId = Guid.NewGuid(), EventId = eventId, BookingNumber = "B1", HoldToken = "h" },
            tickets: new Ticket { Id = Guid.NewGuid(), BookingId = bookingId, EventId = eventId, TicketNo = ticketNo, QrTokenHash = "hash" });

        var allowed = await sut.CanManageTicketAsync(Context(attackerId, UserRole.EventOrganizer), ticketNo);

        Assert.False(allowed);
    }

    [Fact]
    public async Task Admin_can_manage_event_without_impersonation_headers()
    {
        var adminId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var sut = Create();

        var allowed = await sut.CanManageEventAsync(Context(adminId, UserRole.Admin), eventId);

        Assert.True(allowed);
    }

    private static KlegarAuthorizationService Create(
        Event? events = null,
        Booking? bookings = null,
        Ticket? tickets = null)
        => new(
            new FakeEventRepository(events),
            new FakeBookingRepository(bookings),
            new FakeTicketRepository(tickets),
            new RequestUserResolver());

    private static DefaultHttpContext Context(Guid userId, UserRole role)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
            },
            authenticationType: "test"));
        return context;
    }

    private sealed class FakeEventRepository(Event? value) : IEventRepository
    {
        public Task<IReadOnlyList<Event>> GetPublishedAsync(EventSearchRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Event>>(value is null ? Array.Empty<Event>() : new[] { value });
        public Task<IReadOnlyList<Event>> GetByOrganizerUserIdAsync(Guid organizerUserId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Event>>(value is not null && value.OrganizerUserId == organizerUserId ? new[] { value } : Array.Empty<Event>());
        public Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(value?.Id == eventId ? value : null);
        public Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeBookingRepository(Booking? value) : IBookingRepository
    {
        public Task<Booking?> GetByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.FromResult(value?.Id == bookingId ? value : null);
        public Task<IReadOnlyList<Booking>> GetByCustomerAsync(Guid customerUserId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Booking>>(value is not null && value.CustomerUserId == customerUserId ? new[] { value } : Array.Empty<Booking>());
        public Task<IReadOnlyList<Guid>> GetSeatIdsAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
        public Task AddAsync(Booking booking, IReadOnlyCollection<BookingSeat> bookingSeats, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeTicketRepository(Ticket? value) : ITicketRepository
    {
        public Task<IReadOnlyList<Ticket>> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Ticket>>(value?.BookingId == bookingId ? new[] { value } : Array.Empty<Ticket>());
        public Task<IReadOnlyList<Ticket>> AddIfBookingHasNoneAsync(Guid bookingId, IReadOnlyCollection<Ticket> tickets, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Ticket>>(tickets.ToArray());
        public Task<TicketCheckInAttempt> TryCheckInAsync(Guid ticketId, string qrTokenHash, Guid eventId, Guid scannedByUserId, string gate, DateTime nowUtc, CancellationToken cancellationToken = default)
            => Task.FromResult(new TicketCheckInAttempt(null, CheckInResult.Invalid, nowUtc));
        public Task<Ticket?> GetByTicketNoAsync(string ticketNo, CancellationToken cancellationToken = default)
            => Task.FromResult(value?.TicketNo == ticketNo ? value : null);
        public Task<bool> CancelAsync(string ticketNo, DateTime nowUtc, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
