using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Bookings.DTOs;

public sealed class CreateBookingRequest
{
    public Guid EventId { get; set; }
    public string HoldToken { get; set; } = string.Empty;
    public List<Guid> SeatIds { get; set; } = new();
}

public sealed class BookingResponse
{
    public Guid BookingId { get; set; }
    public string BookingNumber { get; set; } = string.Empty;
    public Guid CustomerUserId { get; set; }
    public Guid EventId { get; set; }
    public IReadOnlyList<Guid> SeatIds { get; set; } = Array.Empty<Guid>();
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
}
