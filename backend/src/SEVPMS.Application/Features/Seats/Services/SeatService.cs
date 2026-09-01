using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Application.Features.Seats.Services;
public sealed class SeatService(ISeatInventoryRepository repository, ISeatRealtimeNotifier realtime, TimeProvider timeProvider) : ISeatService
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(5);
    private const int MaxSeatsPerHold = 10;

    public async Task<IReadOnlyList<SeatAvailabilityDto>> GetAvailabilityAsync(Guid eventId, Guid? sectionId, CancellationToken cancellationToken = default)
    {
        if (eventId == Guid.Empty) throw new ArgumentException("Event id is required.", nameof(eventId));
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var items = await repository.GetAvailabilityAsync(eventId, sectionId, now, cancellationToken);
        return items.Select(MapAvailability).ToArray();
    }

    public async Task<SeatHoldResponse> HoldAsync(Guid eventId, Guid userId, CreateSeatHoldRequest request, CancellationToken cancellationToken = default)
    {
        if (eventId == Guid.Empty || userId == Guid.Empty) return Failure("invalid_request", "Event and user are required.");
        var seatIds = (request.SeatIds ?? Array.Empty<Guid>()).Where(x => x != Guid.Empty).Distinct().ToArray();
        if (seatIds.Length == 0) return Failure("no_seats", "Select at least one seat.");
        if (seatIds.Length > MaxSeatsPerHold) return Failure("too_many_seats", $"A single hold supports at most {MaxSeatsPerHold} seats.");
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiry = now.Add(HoldDuration);
        var attempt = await repository.TryCreateOrRefreshHoldAsync(eventId, userId, seatIds, request.ExistingHoldToken, now, expiry, cancellationToken);
        if (!attempt.Succeeded)
            return new(false, null, attempt.ConflictingSeatIds, "seat_conflict", "One or more seats are no longer available.");
        await realtime.PublishSeatStateChangedAsync(eventId, attempt.SeatIds, "Held", attempt.ExpiresAtUtc, cancellationToken);
        return new(true, new(attempt.HoldToken, attempt.EventId, attempt.SeatIds, attempt.ExpiresAtUtc), Array.Empty<Guid>());
    }

    public async Task<bool> ReleaseHoldAsync(string holdToken, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(holdToken) || userId == Guid.Empty) return false;
        var result = await repository.ReleaseHoldAsync(holdToken.Trim(), userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        if (result.Changed) await realtime.PublishSeatStateChangedAsync(result.EventId, result.SeatIds, "Available", null, cancellationToken);
        return result.Changed;
    }

    public async Task<bool> CommitHoldAsync(string holdToken, Guid userId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(holdToken) || userId == Guid.Empty || bookingId == Guid.Empty) return false;
        var result = await repository.CommitHoldAsync(holdToken.Trim(), userId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        if (result.Changed) await realtime.PublishSeatStateChangedAsync(result.EventId, result.SeatIds, "Booked", null, cancellationToken);
        return result.Changed;
    }

    public async Task<SeatAvailabilityDto> UpsertSeatAsync(Guid eventId, UpsertSeatRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SeatStatus>(request.Status, true, out var status)) throw new ArgumentException("Invalid seat status.");
        if (request.SectionId == Guid.Empty || string.IsNullOrWhiteSpace(request.RowLabel) || string.IsNullOrWhiteSpace(request.SeatNumber)) throw new ArgumentException("Section, row and seat number are required.");
        var seat = new Seat { Id = request.SeatId ?? Guid.NewGuid(), EventId = eventId, SectionId = request.SectionId, RowLabel = request.RowLabel.Trim(), SeatNumber = request.SeatNumber.Trim(), X = request.X, Y = request.Y, TicketTypeId = request.TicketTypeId, IsAccessible = request.IsAccessible, Status = status, SeatViewAssetId = request.SeatViewAssetId };
        var saved = await repository.UpsertSeatAsync(eventId, seat, cancellationToken);
        return MapAvailability(new(saved, null));
    }

    public async Task<SeatViewAssetDto?> GetSeatViewAsync(Guid eventId, Guid seatId, CancellationToken cancellationToken = default)
    {
        var asset = await repository.GetSeatViewAsync(eventId, seatId, cancellationToken);
        return asset is null ? null : MapView(asset);
    }

    public async Task<SeatViewAssetDto> UpsertSeatViewAsync(Guid eventId, UpsertSeatViewAssetRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.MediaUrl)) throw new ArgumentException("Media URL is required.");
        if (request.SectionId is null && string.IsNullOrWhiteSpace(request.RowLabel) && request.SeatId is null) throw new ArgumentException("Map the view to a seat, row or section.");
        var asset = new SeatViewAsset { Id = request.Id ?? Guid.NewGuid(), EventId = eventId, SectionId = request.SectionId, RowLabel = string.IsNullOrWhiteSpace(request.RowLabel) ? null : request.RowLabel.Trim().ToUpperInvariant(), SeatId = request.SeatId, MediaUrl = request.MediaUrl.Trim(), ViewerType = string.IsNullOrWhiteSpace(request.ViewerType) ? "panorama" : request.ViewerType.Trim(), DefaultYaw = request.DefaultYaw, DefaultPitch = request.DefaultPitch, DefaultFov = request.DefaultFov, IsRepresentative = request.IsRepresentative };
        return MapView(await repository.UpsertSeatViewAsync(eventId, asset, cancellationToken));
    }

    private static SeatAvailabilityDto MapAvailability(SeatInventorySnapshot item)
    {
        var state = item.Seat.Status switch { SeatStatus.Booked => "Booked", SeatStatus.Blocked => "Blocked", _ when item.ActiveHoldExpiresAtUtc.HasValue => "Held", _ => "Available" };
        return new(item.Seat.Id, item.Seat.EventId, item.Seat.SectionId, item.Seat.RowLabel, item.Seat.SeatNumber, item.Seat.X, item.Seat.Y, item.Seat.TicketTypeId, item.Seat.IsAccessible, state, item.ActiveHoldExpiresAtUtc);
    }
    private static SeatViewAssetDto MapView(SeatViewAsset a) => new(a.Id, a.EventId, a.SectionId, a.RowLabel, a.SeatId, a.MediaUrl, a.ViewerType, a.DefaultYaw, a.DefaultPitch, a.DefaultFov, a.IsRepresentative);
    private static SeatHoldResponse Failure(string code, string message) => new(false, null, Array.Empty<Guid>(), code, message);
}

