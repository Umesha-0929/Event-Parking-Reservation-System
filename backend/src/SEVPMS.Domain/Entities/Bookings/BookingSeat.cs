using SEVPMS.Domain.Common;

namespace SEVPMS.Domain.Entities.Bookings;

public sealed class BookingSeat : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid SeatId { get; set; }
}
