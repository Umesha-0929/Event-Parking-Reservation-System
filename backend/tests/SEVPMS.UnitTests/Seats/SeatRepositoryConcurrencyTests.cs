using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Persistence.Repositories.Seats;
using Xunit;

namespace SEVPMS.UnitTests.Seats;

public sealed class SeatRepositoryConcurrencyTests
{
    [Fact]
    public async Task SecondCustomer_CannotHoldSameActiveSeat()
    {
        await using var db = await CreateDbAsync();

        var eventId = Guid.NewGuid();
        var seat = NewSeat(eventId);

        await SeedSeatAsync(db, seat);

        var repo = new EfSeatInventoryRepository(db);
        var now = DateTime.UtcNow;

        var first = await repo.TryCreateOrRefreshHoldAsync(
            eventId,
            Guid.NewGuid(),
            new[] { seat.Id },
            null,
            now,
            now.AddMinutes(5));

        var second = await repo.TryCreateOrRefreshHoldAsync(
            eventId,
            Guid.NewGuid(),
            new[] { seat.Id },
            null,
            now.AddSeconds(1),
            now.AddMinutes(6));

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Contains(seat.Id, second.ConflictingSeatIds);
    }

    [Fact]
    public async Task ExpiredHold_DoesNotBlockNewCustomer()
    {
        await using var db = await CreateDbAsync();

        var eventId = Guid.NewGuid();
        var seat = NewSeat(eventId);

        await SeedSeatAsync(db, seat);

        db.Set<SeatHold>().Add(new SeatHold
        {
            EventId = eventId,
            SeatId = seat.Id,
            UserId = Guid.NewGuid(),
            HoldToken = "OLD",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1),
            Status = SeatHoldStatus.Active
        });

        await db.SaveChangesAsync();

        var repo = new EfSeatInventoryRepository(db);
        var now = DateTime.UtcNow;

        var result = await repo.TryCreateOrRefreshHoldAsync(
            eventId,
            Guid.NewGuid(),
            new[] { seat.Id },
            null,
            now,
            now.AddMinutes(5));

        Assert.True(result.Succeeded);

        Assert.Equal(
            SeatHoldStatus.Expired,
            await db.Set<SeatHold>()
                .Where(x => x.HoldToken == "OLD")
                .Select(x => x.Status)
                .SingleAsync());
    }

    private static Seat NewSeat(Guid eventId) => new()
    {
        EventId = eventId,
        SeatingLayoutId = Guid.NewGuid(),
        SectionId = Guid.NewGuid(),
        RowLabel = "A",
        RowNumber = 1,
        ColumnNumber = 1,
        SeatNumber = "1",
        Status = SeatStatus.Available
    };

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
                {seat.CreatedAtUtc},
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
        var connection = new SqliteConnection("Data Source=:memory:");
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
