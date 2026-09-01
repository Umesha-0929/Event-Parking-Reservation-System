using System.Data;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
namespace SEVPMS.Infrastructure.Persistence.Repositories.Seats;
public sealed class EfSeatInventoryRepository(SEVPMSDbContext db) : ISeatInventoryRepository
{
    public async Task<IReadOnlyList<SeatInventorySnapshot>> GetAvailabilityAsync(Guid eventId, Guid? sectionId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        await ExpireAsync(nowUtc, cancellationToken);
        var query = db.Set<Seat>().AsNoTracking().Where(x => x.EventId == eventId);
        if (sectionId.HasValue) query = query.Where(x => x.SectionId == sectionId.Value);
        var seats = await query.OrderBy(x => x.SectionId).ThenBy(x => x.RowLabel).ThenBy(x => x.SeatNumber).ToListAsync(cancellationToken);
        var ids = seats.Select(x => x.Id).ToArray();
        var holds = await db.Set<SeatHold>().AsNoTracking().Where(h => ids.Contains(h.SeatId) && h.Status == SeatHoldStatus.Active && h.ExpiresAtUtc > nowUtc).ToDictionaryAsync(h => h.SeatId, h => h.ExpiresAtUtc, cancellationToken);
        return seats.Select(s => new SeatInventorySnapshot(s, holds.TryGetValue(s.Id, out var e) ? e : null)).ToArray();
    }

    public async Task<SeatHoldAttempt> TryCreateOrRefreshHoldAsync(Guid eventId, Guid userId, IReadOnlyCollection<Guid> seatIds, string? existingHoldToken, DateTime nowUtc, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ExpireAsync(nowUtc, cancellationToken);
        var ids = seatIds.Distinct().ToArray();
        var seats = await db.Set<Seat>().Where(x => x.EventId == eventId && ids.Contains(x.Id)).ToListAsync(cancellationToken);
        var found = seats.Select(x => x.Id).ToHashSet();
        var conflicts = ids.Where(id => !found.Contains(id)).ToHashSet();
        foreach (var seat in seats) if (seat.Status != SeatStatus.Available) conflicts.Add(seat.Id);
        var active = await db.Set<SeatHold>().Where(h => ids.Contains(h.SeatId) && h.Status == SeatHoldStatus.Active && h.ExpiresAtUtc > nowUtc).ToListAsync(cancellationToken);
        var token = string.IsNullOrWhiteSpace(existingHoldToken) ? $"HLD-{Guid.NewGuid():N}" : existingHoldToken.Trim();
        foreach (var hold in active)
            if (hold.UserId != userId || !string.Equals(hold.HoldToken, token, StringComparison.Ordinal)) conflicts.Add(hold.SeatId);
        if (conflicts.Count > 0) { await tx.RollbackAsync(cancellationToken); return new(false, token, eventId, ids, expiresAtUtc, conflicts.ToArray()); }
        foreach (var id in ids)
        {
            var existing = active.FirstOrDefault(h => h.SeatId == id);
            if (existing is not null) { existing.ExpiresAtUtc = expiresAtUtc; existing.UpdatedAtUtc = nowUtc; }
            else db.Set<SeatHold>().Add(new SeatHold { EventId = eventId, SeatId = id, UserId = userId, HoldToken = token, ExpiresAtUtc = expiresAtUtc, Status = SeatHoldStatus.Active, CreatedAtUtc = nowUtc });
        }
        await db.SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken);
        return new(true, token, eventId, ids, expiresAtUtc, Array.Empty<Guid>());
    }

    public async Task<SeatHoldMutation> ReleaseHoldAsync(string holdToken, Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ExpireAsync(nowUtc, cancellationToken);
        var holds = await db.Set<SeatHold>().Where(h => h.HoldToken == holdToken && h.UserId == userId && h.Status == SeatHoldStatus.Active).ToListAsync(cancellationToken);
        if (holds.Count == 0) { await tx.RollbackAsync(cancellationToken); return new(false, holdToken, Guid.Empty, Array.Empty<Guid>()); }
        foreach (var h in holds) { h.Status = SeatHoldStatus.Released; h.UpdatedAtUtc = nowUtc; }
        await db.SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken);
        return new(true, holdToken, holds[0].EventId, holds.Select(x => x.SeatId).ToArray());
    }

    public async Task<SeatHoldMutation> CommitHoldAsync(string holdToken, Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await ExpireAsync(nowUtc, cancellationToken);
        var holds = await db.Set<SeatHold>().Where(h => h.HoldToken == holdToken && h.UserId == userId && h.Status == SeatHoldStatus.Active && h.ExpiresAtUtc > nowUtc).ToListAsync(cancellationToken);
        if (holds.Count == 0) { await tx.RollbackAsync(cancellationToken); return new(false, holdToken, Guid.Empty, Array.Empty<Guid>()); }
        var seatIds = holds.Select(h => h.SeatId).ToArray();
        var seats = await db.Set<Seat>().Where(s => seatIds.Contains(s.Id) && s.EventId == holds[0].EventId).ToListAsync(cancellationToken);
        if (seats.Count != seatIds.Length || seats.Any(s => s.Status != SeatStatus.Available)) { await tx.RollbackAsync(cancellationToken); return new(false, holdToken, holds[0].EventId, seatIds); }
        foreach (var h in holds) { h.Status = SeatHoldStatus.Converted; h.UpdatedAtUtc = nowUtc; }
        foreach (var s in seats) { s.Status = SeatStatus.Booked; s.UpdatedAtUtc = nowUtc; }
        await db.SaveChangesAsync(cancellationToken); await tx.CommitAsync(cancellationToken);
        return new(true, holdToken, holds[0].EventId, seatIds);
    }

    public async Task<Seat> UpsertSeatAsync(Guid eventId, Seat seat, CancellationToken cancellationToken = default)
    {
        var existing = await db.Set<Seat>().FirstOrDefaultAsync(x => x.Id == seat.Id, cancellationToken);
        if (existing is null) { seat.EventId = eventId; db.Set<Seat>().Add(seat); }
        else { if (existing.EventId != eventId) throw new InvalidOperationException("Seat belongs to another event."); existing.SectionId = seat.SectionId; existing.RowLabel = seat.RowLabel; existing.SeatNumber = seat.SeatNumber; existing.X = seat.X; existing.Y = seat.Y; existing.TicketTypeId = seat.TicketTypeId; existing.IsAccessible = seat.IsAccessible; existing.Status = seat.Status; existing.SeatViewAssetId = seat.SeatViewAssetId; existing.UpdatedAtUtc = DateTime.UtcNow; seat = existing; }
        await db.SaveChangesAsync(cancellationToken); return seat;
    }

    public async Task<SeatViewAsset?> GetSeatViewAsync(
        Guid eventId,
        Guid seatId,
        CancellationToken cancellationToken = default)
    {
        var seat = await db.Set<Seat>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == seatId &&
                     x.EventId == eventId,
                cancellationToken);

        if (seat is null)
            return null;

        // 1. Explicit asset assigned directly to this seat.
        if (seat.SeatViewAssetId.HasValue)
        {
            var direct = await db.Set<SeatViewAsset>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == seat.SeatViewAssetId.Value &&
                         x.EventId == eventId,
                    cancellationToken);

            if (direct is not null)
                return direct;
        }

        // 2. Asset mapped specifically to this seat.
        var seatView = await db.Set<SeatViewAsset>()
            .AsNoTracking()
            .Where(x =>
                x.EventId == eventId &&
                x.SeatId == seatId)
            .OrderByDescending(x => x.IsRepresentative)
            .ThenByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (seatView is not null)
            return seatView;

        // 3. Shared panorama mapped to this row inside this section.
        var rowView = await db.Set<SeatViewAsset>()
            .AsNoTracking()
            .Where(x =>
                x.EventId == eventId &&
                x.SectionId == seat.SectionId &&
                x.RowLabel == seat.RowLabel &&
                x.SeatId == null)
            .OrderByDescending(x => x.IsRepresentative)
            .ThenByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (rowView is not null)
            return rowView;

        // 4. Representative panorama for the entire section.
        return await db.Set<SeatViewAsset>()
            .AsNoTracking()
            .Where(x =>
                x.EventId == eventId &&
                x.SectionId == seat.SectionId &&
                x.RowLabel == null &&
                x.SeatId == null)
            .OrderByDescending(x => x.IsRepresentative)
            .ThenByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<SeatViewAsset> UpsertSeatViewAsync(Guid eventId, SeatViewAsset asset, CancellationToken cancellationToken = default)
    {
        var existing = await db.Set<SeatViewAsset>().FirstOrDefaultAsync(x => x.Id == asset.Id, cancellationToken);
        if (existing is null) { asset.EventId = eventId; db.Set<SeatViewAsset>().Add(asset); }
        else { if (existing.EventId != eventId) throw new InvalidOperationException("Seat view belongs to another event."); existing.SectionId = asset.SectionId; existing.RowLabel = asset.RowLabel; existing.SeatId = asset.SeatId; existing.MediaUrl = asset.MediaUrl; existing.ViewerType = asset.ViewerType; existing.DefaultYaw = asset.DefaultYaw; existing.DefaultPitch = asset.DefaultPitch; existing.DefaultFov = asset.DefaultFov; existing.IsRepresentative = asset.IsRepresentative; existing.UpdatedAtUtc = DateTime.UtcNow; asset = existing; }
        await db.SaveChangesAsync(cancellationToken); return asset;
    }

    private async Task ExpireAsync(DateTime nowUtc, CancellationToken ct)
    {
        var expired = await db.Set<SeatHold>().Where(h => h.Status == SeatHoldStatus.Active && h.ExpiresAtUtc <= nowUtc).ToListAsync(ct);
        if (expired.Count == 0) return;
        foreach (var h in expired) { h.Status = SeatHoldStatus.Expired; h.UpdatedAtUtc = nowUtc; }
        await db.SaveChangesAsync(ct);
    }
}

