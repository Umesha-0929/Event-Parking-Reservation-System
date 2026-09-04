using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Features.Notifications.DTOs;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Waitlists.Interfaces;
using SEVPMS.Application.Features.Waitlists.Services;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Entities.Waitlists;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Waitlists;

public sealed class WaitlistServiceTests
{
    [Fact]
    public async Task JoinAsync_CreatesWaitingEntryWithPosition()
    {
        var eventEntity = CreatePublishedEvent();
        var waitlistRepository = new FakeWaitlistRepository();

        var service = CreateService(
            waitlistRepository,
            eventEntity);

        var customerId = Guid.NewGuid();

        var result = await service.JoinAsync(
            customerId,
            eventEntity.Id);

        Assert.Equal(eventEntity.Id, result.EventId);
        Assert.Equal(customerId, result.CustomerUserId);
        Assert.Equal(WaitlistStatus.Waiting, result.Status);
        Assert.Equal(1, result.Position);
        Assert.Single(waitlistRepository.Entries);
    }

    [Fact]
    public async Task JoinAsync_WhenActiveEntryExists_Throws()
    {
        var eventEntity = CreatePublishedEvent();
        var customerId = Guid.NewGuid();

        var waitlistRepository =
            new FakeWaitlistRepository();

        waitlistRepository.Entries.Add(
            new WaitlistEntry
            {
                EventId = eventEntity.Id,
                CustomerUserId = customerId,
                Status = WaitlistStatus.Waiting
            });

        var service = CreateService(
            waitlistRepository,
            eventEntity);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.JoinAsync(
                customerId,
                eventEntity.Id));
    }

    [Fact]
    public async Task LeaveAsync_MarksEntryAsLeft()
    {
        var eventEntity = CreatePublishedEvent();
        var customerId = Guid.NewGuid();

        var entry = new WaitlistEntry
        {
            EventId = eventEntity.Id,
            CustomerUserId = customerId,
            Status = WaitlistStatus.Waiting
        };

        var waitlistRepository =
            new FakeWaitlistRepository();

        waitlistRepository.Entries.Add(entry);

        var service = CreateService(
            waitlistRepository,
            eventEntity);

        var result = await service.LeaveAsync(
            customerId,
            eventEntity.Id);

        Assert.True(result);

        Assert.Equal(
            WaitlistStatus.Left,
            entry.Status);

        Assert.NotNull(entry.LeftAtUtc);
    }

    [Fact]
    public async Task NotifyNextEligibleAsync_SelectsOldestCustomers()
    {
        var eventEntity = CreatePublishedEvent();

        var first = new WaitlistEntry
        {
            EventId = eventEntity.Id,
            CustomerUserId = Guid.NewGuid(),
            Status = WaitlistStatus.Waiting,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-30)
        };

        var second = new WaitlistEntry
        {
            EventId = eventEntity.Id,
            CustomerUserId = Guid.NewGuid(),
            Status = WaitlistStatus.Waiting,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-20)
        };

        var third = new WaitlistEntry
        {
            EventId = eventEntity.Id,
            CustomerUserId = Guid.NewGuid(),
            Status = WaitlistStatus.Waiting,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
        };

        var waitlistRepository =
            new FakeWaitlistRepository();

        waitlistRepository.Entries.AddRange(
            new[]
            {
                first,
                second,
                third
            });

        var notificationService =
            new FakeNotificationService();

        var service =
            new WaitlistService(
                waitlistRepository,
                new FakeEventRepository(eventEntity),
                notificationService);

        var result =
            await service.NotifyNextEligibleAsync(
                eventEntity.Id,
                2);

        Assert.Equal(2, result.EligibleCount);

        Assert.Equal(
            WaitlistStatus.Eligible,
            first.Status);

        Assert.Equal(
            WaitlistStatus.Eligible,
            second.Status);

        Assert.Equal(
            WaitlistStatus.Waiting,
            third.Status);

        Assert.Equal(
            2,
            notificationService.Created.Count);

        Assert.Contains(
            first.CustomerUserId,
            result.CustomerUserIds);

        Assert.Contains(
            second.CustomerUserId,
            result.CustomerUserIds);
    }

    private static WaitlistService CreateService(
        FakeWaitlistRepository waitlistRepository,
        Event eventEntity)
        => new(
            waitlistRepository,
            new FakeEventRepository(eventEntity),
            new FakeNotificationService());

    private static Event CreatePublishedEvent()
        => new()
        {
            Id = Guid.NewGuid(),
            OrganizerUserId = Guid.NewGuid(),
            VenueId = Guid.NewGuid(),
            Title = "Test Event",
            Description = "Test",
            Category = "Music",
            StartAtUtc = DateTime.UtcNow.AddDays(1),
            EndAtUtc = DateTime.UtcNow.AddDays(1).AddHours(3),
            Status = EventStatus.Published
        };

    private sealed class FakeWaitlistRepository
        : IWaitlistRepository
    {
        public List<WaitlistEntry> Entries { get; }
            = new();

        public Task<WaitlistEntry?> GetByIdAsync(
            Guid waitlistEntryId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                Entries.SingleOrDefault(
                    x => x.Id == waitlistEntryId));

        public Task<WaitlistEntry?>
            GetByEventAndCustomerAsync(
                Guid eventId,
                Guid customerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                Entries.SingleOrDefault(
                    x =>
                        x.EventId == eventId &&
                        x.CustomerUserId == customerUserId));

        public Task<IReadOnlyList<WaitlistEntry>>
            GetByEventAsync(
                Guid eventId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<WaitlistEntry>>(
                Entries
                    .Where(x => x.EventId == eventId)
                    .ToList());

        public Task AddAsync(
            WaitlistEntry entry,
            CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeEventRepository(
        Event eventEntity)
        : IEventRepository
    {
        public Task<IReadOnlyList<Event>>
            GetPublishedAsync(
                EventSearchRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                new[] { eventEntity });

        public Task<IReadOnlyList<Event>>
            GetByOrganizerUserIdAsync(
                Guid organizerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Event>>(
                eventEntity.OrganizerUserId == organizerUserId
                    ? new[] { eventEntity }
                    : Array.Empty<Event>());

        public Task<Event?> GetByIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Event?>(
                eventEntity.Id == eventId
                    ? eventEntity
                    : null);

        public Task AddAsync(
            Event eventEntity,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeNotificationService
        : INotificationService
    {
        public List<NotificationResponse> Created { get; }
            = new();

        public Task<IReadOnlyList<NotificationResponse>>
            GetMineAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<NotificationResponse>>(
                Created
                    .Where(x => x.UserId == userId)
                    .ToList());

        public Task<NotificationResponse> MarkReadAsync(
            Guid userId,
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            var result =
                Created.Single(
                    x =>
                        x.UserId == userId &&
                        x.NotificationId == notificationId);

            result.IsRead = true;

            return Task.FromResult(result);
        }

        public Task<NotificationResponse> CreateAsync(
            Guid userId,
            string title,
            string message,
            string type,
            CancellationToken cancellationToken = default)
        {
            var notification =
                new NotificationResponse
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    CreatedAtUtc = DateTime.UtcNow
                };

            Created.Add(notification);

            return Task.FromResult(notification);
        }
    }
}