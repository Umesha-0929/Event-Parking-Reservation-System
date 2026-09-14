using System.Globalization;
using System.Text;
using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Calendar.DTOs;
using SEVPMS.Application.Features.Calendar.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Calendar.Services;

public sealed class BookingCalendarService(
    IBookingRepository bookingRepository,
    IEventRepository eventRepository,
    IVenueRepository venueRepository)
    : IBookingCalendarService
{
    public async Task<BookingCalendarExport> GetAsync(
        Guid customerUserId,
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking =
            await bookingRepository.GetByIdAsync(
                bookingId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Booking was not found.");

        if (booking.CustomerUserId != customerUserId)
        {
            throw new ForbiddenAccessException(
                "You do not own this booking.");
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException(
                "Calendar export is available only for confirmed bookings.");
        }

        var eventEntity =
            await eventRepository.GetByIdAsync(
                booking.EventId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Event was not found.");

        if (eventEntity.Status == EventStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Calendar export is not available for a cancelled event.");
        }

        var venue =
            await venueRepository.GetByIdAsync(
                eventEntity.VenueId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Venue was not found.");

        var location =
            BuildLocation(
                venue.Name,
                venue.AddressLine1,
                venue.AddressLine2,
                venue.City,
                venue.District,
                venue.Country);

        var startUtc =
            NormalizeUtc(
                eventEntity.StartAtUtc);

        var endUtc =
            NormalizeUtc(
                eventEntity.EndAtUtc);

        var googleUrl =
            BuildGoogleCalendarUrl(
                eventEntity.Title,
                eventEntity.Description,
                booking.BookingNumber,
                location,
                startUtc,
                endUtc);

        var fileName =
            BuildFileName(
                eventEntity.Title,
                booking.BookingNumber);

        var ics =
            BuildIcs(
                booking.Id,
                booking.BookingNumber,
                eventEntity.Id,
                eventEntity.Title,
                eventEntity.Description,
                location,
                startUtc,
                endUtc);

        return new BookingCalendarExport
        {
            Info = new BookingCalendarResponse
            {
                BookingId = booking.Id,
                EventId = eventEntity.Id,
                EventTitle = eventEntity.Title,
                VenueName = venue.Name,
                Location = location,
                StartAtUtc = startUtc,
                EndAtUtc = endUtc,
                GoogleCalendarUrl = googleUrl,
                IcsDownloadPath =
                    $"/api/bookings/{booking.Id}/calendar.ics"
            },

            FileName = fileName,

            IcsContent = ics
        };
    }

    private static string BuildGoogleCalendarUrl(
        string title,
        string description,
        string bookingNumber,
        string location,
        DateTime startUtc,
        DateTime endUtc)
    {
        var dates =
            $"{FormatGoogleDate(startUtc)}/{FormatGoogleDate(endUtc)}";

        var details =
            string.IsNullOrWhiteSpace(description)
                ? $"SEVPMS Booking: {bookingNumber}"
                : $"{description}\n\nSEVPMS Booking: {bookingNumber}";

        return
            "https://calendar.google.com/calendar/render" +
            "?action=TEMPLATE" +
            $"&text={Uri.EscapeDataString(title)}" +
            $"&dates={Uri.EscapeDataString(dates)}" +
            $"&details={Uri.EscapeDataString(details)}" +
            $"&location={Uri.EscapeDataString(location)}";
    }

    private static string BuildIcs(
        Guid bookingId,
        string bookingNumber,
        Guid eventId,
        string title,
        string description,
        string location,
        DateTime startUtc,
        DateTime endUtc)
    {
        var uid =
            $"booking-{bookingId:N}-event-{eventId:N}@sevpms";

        var details =
            string.IsNullOrWhiteSpace(description)
                ? $"SEVPMS Booking: {bookingNumber}"
                : $"{description}\nSEVPMS Booking: {bookingNumber}";

        var lines =
            new[]
            {
                "BEGIN:VCALENDAR",
                "VERSION:2.0",
                "PRODID:-//SEVPMS//Event Calendar//EN",
                "CALSCALE:GREGORIAN",
                "METHOD:PUBLISH",
                "BEGIN:VEVENT",
                $"UID:{uid}",
                $"DTSTAMP:{FormatIcsDate(DateTime.UtcNow)}",
                $"DTSTART:{FormatIcsDate(startUtc)}",
                $"DTEND:{FormatIcsDate(endUtc)}",
                $"SUMMARY:{EscapeIcs(title)}",
                $"DESCRIPTION:{EscapeIcs(details)}",
                $"LOCATION:{EscapeIcs(location)}",
                "STATUS:CONFIRMED",
                "TRANSP:OPAQUE",
                "END:VEVENT",
                "END:VCALENDAR"
            };

        return string.Join(
                   "\r\n",
                   lines)
               + "\r\n";
    }

    private static string BuildLocation(
        string venueName,
        string addressLine1,
        string? addressLine2,
        string city,
        string district,
        string country)
    {
        return string.Join(
            ", ",
            new[]
            {
                venueName,
                addressLine1,
                addressLine2,
                city,
                district,
                country
            }
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim()));
    }

    private static string BuildFileName(
        string eventTitle,
        string bookingNumber)
    {
        var safeTitle =
            new string(
                eventTitle
                    .Where(
                        c =>
                            char.IsLetterOrDigit(c) ||
                            c == '-' ||
                            c == '_')
                    .ToArray());

        if (string.IsNullOrWhiteSpace(safeTitle))
        {
            safeTitle = "event";
        }

        return
            $"{safeTitle}-{bookingNumber}.ics";
    }

    private static string EscapeIcs(
        string value)
    {
        return value
            .Replace(
                "\\",
                "\\\\",
                StringComparison.Ordinal)
            .Replace(
                "\r\n",
                "\\n",
                StringComparison.Ordinal)
            .Replace(
                "\n",
                "\\n",
                StringComparison.Ordinal)
            .Replace(
                ";",
                "\\;",
                StringComparison.Ordinal)
            .Replace(
                ",",
                "\\,",
                StringComparison.Ordinal);
    }

    private static DateTime NormalizeUtc(
        DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc);
    }

    private static string FormatGoogleDate(
        DateTime value)
    {
        return value.ToString(
            "yyyyMMdd'T'HHmmss'Z'",
            CultureInfo.InvariantCulture);
    }

    private static string FormatIcsDate(
        DateTime value)
    {
        return NormalizeUtc(value)
            .ToString(
                "yyyyMMdd'T'HHmmss'Z'",
                CultureInfo.InvariantCulture);
    }
}