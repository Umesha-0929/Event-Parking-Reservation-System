using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Seats.Services;
using SEVPMS.Application.Features.Tickets.DTOs;
using SEVPMS.Application.Features.Tickets.Interfaces;
using SEVPMS.Application.Features.Tickets.Services;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Persistence.Repositories.Seats;
using SEVPMS.Infrastructure.Persistence.Repositories.Tickets;
using Xunit;

namespace SEVPMS.UnitTests.Seats;

public sealed class SeatBookingTicketFlowTests
{
    [Fact]
    public async Task HeldSeat_CanBecomeBookedAndGenerateUsableTicket()
    {
        await using var db = await CreateDbAsync();

        var eventId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var scannerId = Guid.NewGuid();

        var seat = new Seat
        {
            EventId = eventId,
            SeatingLayoutId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            RowLabel = "A",
            RowNumber = 1,
            ColumnNumber = 1,
            SeatNumber = "1",
            X = 100,
            Y = 200,
            Status = SeatStatus.Available
        };

        await SeedSeatAsync(db, seat);

        var seatRepository =
            new EfSeatInventoryRepository(db);

        var seatService =
            new SeatService(
                seatRepository,
                new FakeSeatRealtime(),
                TimeProvider.System);

        var hold = await seatService.HoldAsync(
            eventId,
            customerId,
            new CreateSeatHoldRequest(
                new[] { seat.Id }));

        Assert.True(hold.Succeeded);
        Assert.NotNull(hold.Hold);

        var heldAvailability =
            await seatService.GetAvailabilityAsync(
                eventId,
                null);

        Assert.Equal(
            "Held",
            Assert.Single(heldAvailability).State);

        var committed =
            await seatService.CommitHoldAsync(
                hold.Hold!.HoldToken,
                customerId,
                bookingId);

        Assert.True(committed);

        var bookedSeat =
            await db.Set<Seat>()
                .AsNoTracking()
                .SingleAsync(x => x.Id == seat.Id);

        Assert.Equal(
            SeatStatus.Booked,
            bookedSeat.Status);

        var convertedHold =
            await db.Set<SeatHold>()
                .AsNoTracking()
                .SingleAsync(
                    x => x.HoldToken ==
                         hold.Hold.HoldToken);

        Assert.Equal(
            SeatHoldStatus.Converted,
            convertedHold.Status);

        var qr = new FakeQr();

        var ticketService =
            new TicketService(
                new EfTicketRepository(db),
                qr,
                new FakeTicketRealtime(),
                TimeProvider.System);

        var tickets =
            await ticketService.IssueAsync(
                bookingId,
                new IssueTicketsRequest(
                    eventId,
                    new Guid?[] { seat.Id }));

        var ticket = Assert.Single(tickets);

        Assert.Equal(
            bookingId,
            ticket.BookingId);

        Assert.Equal(
            eventId,
            ticket.EventId);

        Assert.Equal(
            seat.Id,
            ticket.SeatId);

        Assert.Equal(
            "Active",
            ticket.Status);

        Assert.False(
            string.IsNullOrWhiteSpace(
                ticket.QrPayload));

        var firstScan =
            await ticketService.CheckInAsync(
                eventId,
                scannerId,
                new CheckInTicketRequest(
                    ticket.QrPayload,
                    "Gate 1"));

        Assert.True(firstScan.Succeeded);
        Assert.Equal(
            "Accepted",
            firstScan.Result);

        var secondScan =
            await ticketService.CheckInAsync(
                eventId,
                scannerId,
                new CheckInTicketRequest(
                    ticket.QrPayload,
                    "Gate 1"));

        Assert.False(secondScan.Succeeded);
        Assert.Equal(
            "Duplicate",
            secondScan.Result);

        var checkIns =
            await db.Set<SEVPMS.Domain.Entities.Tickets.CheckIn>()
                .AsNoTracking()
                .Where(x =>
                    x.TicketId == ticket.TicketId)
                .OrderBy(x => x.ScannedAtUtc)
                .ToListAsync();

        Assert.Equal(2, checkIns.Count);
        Assert.Equal(
            CheckInResult.Accepted,
            checkIns[0].Result);
        Assert.Equal(
            CheckInResult.Duplicate,
            checkIns[1].Result);
    }

    private static async Task SeedSeatAsync(
        SEVPMSDbContext db,
        Seat seat)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Seats"
            (
                "Id",
                "CreatedAtUtc",
                "UpdatedAtUtc",
                "EventId",
                "SeatingLayoutId",
                "SectionId",
                "SeatCategoryId",
                "RowLabel",
                "RowNumber",
                "ColumnNumber",
                "SeatNumber",
                "X",
                "Y",
                "TicketTypeId",
                "IsAccessible",
                "Status",
                "SeatViewAssetId",
                "RowVersion"
            )
            VALUES
            (
                {seat.Id},
                {DateTime.UtcNow},
                NULL,
                {seat.EventId},
                {seat.SeatingLayoutId},
                {seat.SectionId},
                NULL,
                {seat.RowLabel},
                {seat.RowNumber},
                {seat.ColumnNumber},
                {seat.SeatNumber},
                {seat.X},
                {seat.Y},
                NULL,
                {seat.IsAccessible},
                {seat.Status.ToString()},
                NULL,
                {new byte[8]}
            );
            """);
    }

    private static async Task<SEVPMSDbContext> CreateDbAsync()
    {
        var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<SEVPMSDbContext>()
                .UseSqlite(connection)
                .Options;

        var db =
            new SEVPMSDbContext(options);

        await db.Database.EnsureCreatedAsync();

        return db;
    }

    private sealed class FakeSeatRealtime
        : ISeatRealtimeNotifier
    {
        public Task PublishSeatStateChangedAsync(
            Guid eventId,
            IReadOnlyCollection<Guid> seatIds,
            string state,
            DateTime? expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTicketRealtime
        : ITicketRealtimeNotifier
    {
        public Task PublishCheckInChangedAsync(
            Guid eventId,
            Guid ticketId,
            string state,
            string gate,
            DateTime scannedAtUtc,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeQr
        : ITicketQrTokenService
    {
        public string CreatePayload(
            Guid ticketId)
        {
            return $"T.{ticketId:N}";
        }

        public string HashPayload(
            string payload)
        {
            return Convert.ToHexString(
                    SHA256.HashData(
                        Encoding.UTF8.GetBytes(payload)))
                .ToLowerInvariant();
        }

        public bool TryValidatePayload(
            string payload,
            out Guid ticketId)
        {
            ticketId = Guid.Empty;

            var parts = payload.Split('.');

            return parts.Length == 2 &&
                   Guid.TryParseExact(
                       parts[1],
                       "N",
                       out ticketId);
        }
    }
}
