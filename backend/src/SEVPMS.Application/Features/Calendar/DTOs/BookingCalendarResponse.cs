namespace SEVPMS.Application.Features.Calendar.DTOs;

public sealed class BookingCalendarResponse
{
    public Guid BookingId { get; set; }

    public Guid EventId { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public string VenueName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public string GoogleCalendarUrl { get; set; } = string.Empty;

    public string IcsDownloadPath { get; set; } = string.Empty;
}

public sealed class BookingCalendarExport
{
    public BookingCalendarResponse Info { get; set; } = new();

    public string FileName { get; set; } = string.Empty;

    public string IcsContent { get; set; } = string.Empty;
}