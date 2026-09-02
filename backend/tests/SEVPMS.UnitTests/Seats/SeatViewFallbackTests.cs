using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Persistence.Repositories.Seats;
using Xunit;

namespace SEVPMS.UnitTests.Seats;

public sealed class SeatViewFallbackTests
{
    [Fact]
    public async Task SeatSpecificView_TakesPriorityOverRowAndSectionViews()
    {
        await using var db = await CreateDbAsync();

        var eventId = Guid.NewGuid();
        var seat = NewSeat(eventId);

        await SeedSeatAsync(db, seat);

        db.Set<SeatViewAsset>().AddRange(
            NewSectionView(eventId, seat.SectionId, "section.jpg"),
            NewRowView(eventId, seat.SectionId, seat.RowLabel, "row.jpg"),
            NewSeatView(eventId, seat.Id, "seat.jpg"));

        await db.SaveChangesAsync();

        var repository = new EfSeatInventoryRepository(db);

        var result = await repository.GetSeatViewAsync(
            eventId,
            seat.Id);

        Assert.NotNull(result);
        Assert.Equal("seat.jpg", result.MediaUrl);
        Assert.Equal(seat.Id, result.SeatId);
    }

    [Fact]
    public async Task RowView_IsUsedWhenSeatSpecificViewDoesNotExist()
    {
        await using var db = await CreateDbAsync();

        var eventId = Guid.NewGuid();
        var seat = NewSeat(eventId);

        await SeedSeatAsync(db, seat);

        db.Set<SeatViewAsset>().AddRange(
            NewSectionView(eventId, seat.SectionId, "section.jpg"),
            NewRowView(eventId, seat.SectionId, seat.RowLabel, "row.jpg"));

        await db.SaveChangesAsync();

        var repository = new EfSeatInventoryRepository(db);

        var result = await repository.GetSeatViewAsync(
            eventId,
            seat.Id);

        Assert.NotNull(result);
        Assert.Equal("row.jpg", result.MediaUrl);
        Assert.Equal(seat.RowLabel, result.RowLabel);
        Assert.Null(result.SeatId);
    }

    [Fact]
    public async Task SectionView_IsUsedWhenSeatAndRowViewsDoNotExist()
    {
        await using var db = await CreateDbAsync();

        var eventId = Guid.NewGuid();
        var seat = NewSeat(eventId);

        await SeedSeatAsync(db, seat);

        db.Set<SeatViewAsset>().Add(
            NewSectionView(
                eventId,
                seat.SectionId,
                "section.jpg"));

        await db.SaveChangesAsync();

        var repository = new EfSeatInventoryRepository(db);

        var result = await repository.GetSeatViewAsync(
            eventId,
            seat.Id);

        Assert.NotNull(result);
        Assert.Equal("section.jpg", result.MediaUrl);
        Assert.Equal(seat.SectionId, result.SectionId);
        Assert.Null(result.RowLabel);
        Assert.Null(result.SeatId);
    }

    private static Seat NewSeat(Guid eventId)
    {
        return new Seat
        {
            EventId = eventId,
            SeatingLayoutId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            RowLabel = "B",
            RowNumber = 2,
            ColumnNumber = 4,
            SeatNumber = "4",
            Status = SeatStatus.Available
        };
    }

    private static SeatViewAsset NewSeatView(
        Guid eventId,
        Guid seatId,
        string mediaUrl)
    {
        return new SeatViewAsset
        {
            EventId = eventId,
            SeatId = seatId,
            MediaUrl = mediaUrl,
            ViewerType = "panorama",
            IsRepresentative = false
        };
    }

    private static SeatViewAsset NewRowView(
        Guid eventId,
        Guid sectionId,
        string rowLabel,
        string mediaUrl)
    {
        return new SeatViewAsset
        {
            EventId = eventId,
            SectionId = sectionId,
            RowLabel = rowLabel,
            MediaUrl = mediaUrl,
            ViewerType = "panorama",
            IsRepresentative = true
        };
    }

    private static SeatViewAsset NewSectionView(
        Guid eventId,
        Guid sectionId,
        string mediaUrl)
    {
        return new SeatViewAsset
        {
            EventId = eventId,
            SectionId = sectionId,
            MediaUrl = mediaUrl,
            ViewerType = "panorama",
            IsRepresentative = true
        };
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
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<SEVPMSDbContext>()
                .UseSqlite(connection)
                .Options;

        var db = new SEVPMSDbContext(options);

        await db.Database.EnsureCreatedAsync();

        return db;
    }
}
