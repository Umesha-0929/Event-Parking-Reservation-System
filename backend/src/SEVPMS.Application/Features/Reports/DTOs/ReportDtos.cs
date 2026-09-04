namespace SEVPMS.Application.Features.Reports.DTOs;

public sealed class ReportDateRange
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}

public sealed class PlatformReportResponse
{
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }

    public int Users { get; set; }

    public int Events { get; set; }

    public int PublishedEvents { get; set; }

    public int Venues { get; set; }

    public int Bookings { get; set; }

    public int ConfirmedBookings { get; set; }

    public int SuccessfulPayments { get; set; }

    public int Refunds { get; set; }

    public int Attendance { get; set; }

    public int ParkingReservations { get; set; }

    public int FoodOrders { get; set; }

    public decimal GrossRevenue { get; set; }

    public decimal RefundedAmount { get; set; }

    public decimal NetRevenue { get; set; }

    public decimal FoodRevenue { get; set; }
}

public sealed class OrganizerReportResponse
{
    public Guid OrganizerUserId { get; set; }

    public int Events { get; set; }

    public int PublishedEvents { get; set; }

    public int ConfirmedBookings { get; set; }

    public int Attendance { get; set; }

    public int ParkingReservations { get; set; }

    public int FoodOrders { get; set; }

    public decimal Revenue { get; set; }

    public decimal FoodRevenue { get; set; }
}

public sealed class VenueOwnerReportResponse
{
    public Guid VenueOwnerUserId { get; set; }

    public int Venues { get; set; }

    public int RentalRequests { get; set; }

    public int AcceptedRentals { get; set; }

    public decimal AcceptedRentalValue { get; set; }
}