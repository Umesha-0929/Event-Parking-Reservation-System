namespace SEVPMS.Application.Features.Seats.DTOs;
public sealed record SeatAvailabilityDto(
    Guid SeatId, Guid EventId, Guid SectionId, string RowLabel, string SeatNumber,
    decimal X, decimal Y, Guid? TicketTypeId, bool IsAccessible, string State, DateTime? HeldUntilUtc);
public sealed record CreateSeatHoldRequest(IReadOnlyCollection<Guid> SeatIds, string? ExistingHoldToken = null);
public sealed record SeatHoldDto(string HoldToken, Guid EventId, IReadOnlyCollection<Guid> SeatIds, DateTime ExpiresAtUtc);
public sealed record SeatHoldResponse(bool Succeeded, SeatHoldDto? Hold, IReadOnlyCollection<Guid> ConflictingSeatIds, string? ErrorCode = null, string? Message = null);
public sealed record UpsertSeatRequest(Guid? SeatId, Guid SectionId, string RowLabel, string SeatNumber, decimal X, decimal Y, Guid? TicketTypeId, bool IsAccessible, string Status = "Available", Guid? SeatViewAssetId = null);
public sealed record SeatViewAssetDto(Guid AssetId, Guid EventId, Guid? SectionId, Guid? SeatId, string MediaUrl, string ViewerType, decimal? DefaultYaw, decimal? DefaultPitch, decimal? DefaultFov, bool IsRepresentative);
public sealed record UpsertSeatViewAssetRequest(Guid? AssetId, Guid? SectionId, Guid? SeatId, string MediaUrl, string ViewerType = "panorama", decimal? DefaultYaw = null, decimal? DefaultPitch = null, decimal? DefaultFov = null, bool IsRepresentative = true);
