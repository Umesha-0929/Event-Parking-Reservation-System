using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.Tickets;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Persistence.Repositories.Tickets;
namespace SEVPMS.UnitTests.Tickets;
public sealed class TicketRepositoryCheckInTests
{
    [Fact]
    public async Task DuplicateQrScan_IsRejectedAndRecorded()
    {
        await using var db = await CreateDbAsync();
        var eventId = Guid.NewGuid();
        var ticket = new Ticket { BookingId = Guid.NewGuid(), EventId = eventId, TicketNo = "TKT-TEST-1", QrTokenHash = new string('a', 64), Status = TicketStatus.Active, IssuedAtUtc = DateTime.UtcNow };
        db.Set<Ticket>().Add(ticket); await db.SaveChangesAsync();
        var repo = new EfTicketRepository(db); var scanner = Guid.NewGuid(); var now = DateTime.UtcNow;
        var first = await repo.TryCheckInAsync(ticket.Id, ticket.QrTokenHash, eventId, scanner, "Gate 1", now);
        var second = await repo.TryCheckInAsync(ticket.Id, ticket.QrTokenHash, eventId, scanner, "Gate 1", now.AddSeconds(2));
        Assert.Equal(CheckInResult.Accepted, first.Result); Assert.Equal(CheckInResult.Duplicate, second.Result);
        Assert.Equal(2, await db.Set<CheckIn>().CountAsync());
    }

    private static async Task<SEVPMSDbContext> CreateDbAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SEVPMSDbContext>().UseSqlite(connection).Options; var db = new SEVPMSDbContext(options); await db.Database.EnsureCreatedAsync(); return db;
    }
}

