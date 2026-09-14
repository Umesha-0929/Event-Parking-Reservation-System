using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.Waitlists.DTOs;
using SEVPMS.Application.Features.Waitlists.Interfaces;
using SEVPMS.Application.Features.Waitlists.Validators;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Waitlists;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Waitlists.Services;

public sealed class WaitlistService(
    IWaitlistRepository waitlistRepository,
    IEventRepository eventRepository,
    INotificationService notificationService)
    : IWaitlistService
{
    public async Task<WaitlistEntryDto?> GetMineAsync(
        Guid customerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        WaitlistValidator.ValidateCustomerId(customerUserId);
        WaitlistValidator.ValidateEventId(eventId);

        var entry =
            await waitlistRepository.GetByEventAndCustomerAsync(
                eventId,
                customerUserId,
                cancellationToken);

        if (entry is null)
            return null;

        var entries =
            await waitlistRepository.GetByEventAsync(
                eventId,
                cancellationToken);

        return Map(
            entry,
            CalculatePosition(entry, entries));
    }

    public async Task<WaitlistEntryDto> JoinAsync(
        Guid customerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        WaitlistValidator.ValidateCustomerId(customerUserId);
        WaitlistValidator.ValidateEventId(eventId);

        var eventEntity =
            await eventRepository.GetByIdAsync(
                eventId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Event was not found.");

        if (eventEntity.Status != EventStatus.Published)
            throw new InvalidOperationException(
                "Only published events support waitlists.");

        if (eventEntity.EndAtUtc <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "The waitlist is not available for an event that has ended.");

        var existing =
            await waitlistRepository.GetByEventAndCustomerAsync(
                eventId,
                customerUserId,
                cancellationToken);

        if (existing is not null &&
            existing.Status is WaitlistStatus.Waiting
                or WaitlistStatus.Eligible)
        {
            throw new InvalidOperationException(
                "You are already on the waitlist for this event.");
        }

        WaitlistEntry entry;

        if (existing is not null)
        {
            existing.Status = WaitlistStatus.Waiting;
            existing.EligibleAtUtc = null;
            existing.LeftAtUtc = null;
            existing.ConvertedAtUtc = null;
            existing.CreatedAtUtc = DateTime.UtcNow;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            entry = existing;
        }
        else
        {
            entry = new WaitlistEntry
            {
                EventId = eventId,
                CustomerUserId = customerUserId,
                Status = WaitlistStatus.Waiting
            };

            await waitlistRepository.AddAsync(
                entry,
                cancellationToken);
        }

        await waitlistRepository.SaveChangesAsync(
            cancellationToken);

        var entries =
            await waitlistRepository.GetByEventAsync(
                eventId,
                cancellationToken);

        return Map(
            entry,
            CalculatePosition(entry, entries));
    }

    public async Task<bool> LeaveAsync(
        Guid customerUserId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        WaitlistValidator.ValidateCustomerId(customerUserId);
        WaitlistValidator.ValidateEventId(eventId);

        var entry =
            await waitlistRepository.GetByEventAndCustomerAsync(
                eventId,
                customerUserId,
                cancellationToken);

        if (entry is null)
            return false;

        if (entry.Status == WaitlistStatus.Left)
            return false;

        if (entry.Status == WaitlistStatus.Converted)
            throw new InvalidOperationException(
                "A converted waitlist entry cannot be left.");

        entry.Status = WaitlistStatus.Left;
        entry.LeftAtUtc = DateTime.UtcNow;
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await waitlistRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<WaitlistEligibilityResultDto>
        NotifyNextEligibleAsync(
            Guid eventId,
            int availableCount,
            CancellationToken cancellationToken = default)
    {
        WaitlistValidator.ValidateEventId(eventId);
        WaitlistValidator.ValidateAvailableCount(availableCount);

        var eventEntity =
            await eventRepository.GetByIdAsync(
                eventId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Event was not found.");

        if (eventEntity.Status != EventStatus.Published)
            throw new InvalidOperationException(
                "Only published events can release waitlist customers.");

        var entries =
            await waitlistRepository.GetByEventAsync(
                eventId,
                cancellationToken);

        var eligibleEntries = entries
            .Where(x => x.Status == WaitlistStatus.Waiting)
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .Take(availableCount)
            .ToList();

        var now = DateTime.UtcNow;

        foreach (var entry in eligibleEntries)
        {
            entry.Status = WaitlistStatus.Eligible;
            entry.EligibleAtUtc = now;
            entry.UpdatedAtUtc = now;
        }

        if (eligibleEntries.Count > 0)
        {
            await waitlistRepository.SaveChangesAsync(
                cancellationToken);

            foreach (var entry in eligibleEntries)
            {
                await notificationService.CreateAsync(
                    entry.CustomerUserId,
                    "Waitlist spot available",
                    $"A spot is now available for {eventEntity.Title}. You can continue with your booking.",
                    "Waitlist",
                    cancellationToken);
            }
        }

        return new WaitlistEligibilityResultDto
        {
            EventId = eventId,
            RequestedCount = availableCount,
            EligibleCount = eligibleEntries.Count,
            CustomerUserIds = eligibleEntries
                .Select(x => x.CustomerUserId)
                .ToList()
        };
    }

    private static int? CalculatePosition(
        WaitlistEntry entry,
        IReadOnlyList<WaitlistEntry> entries)
    {
        if (entry.Status != WaitlistStatus.Waiting)
            return null;

        var activeEntries = entries
            .Where(x => x.Status == WaitlistStatus.Waiting)
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .ToList();

        var index = activeEntries.FindIndex(
            x => x.Id == entry.Id);

        return index < 0
            ? null
            : index + 1;
    }

    private static WaitlistEntryDto Map(
        WaitlistEntry entry,
        int? position)
        => new()
        {
            Id = entry.Id,
            EventId = entry.EventId,
            CustomerUserId = entry.CustomerUserId,
            Status = entry.Status,
            Position = position,
            JoinedAtUtc = entry.CreatedAtUtc,
            EligibleAtUtc = entry.EligibleAtUtc
        };
}