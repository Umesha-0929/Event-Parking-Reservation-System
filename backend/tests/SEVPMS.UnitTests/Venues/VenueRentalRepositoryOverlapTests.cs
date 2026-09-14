using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.UnitTests.Venues;

public sealed class VenueRentalRepositoryOverlapTests
{
    [Fact]
    public async Task Accepted_overlapping_rental_is_detected()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<SEVPMSDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var db =
            new SEVPMSDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var venueId = Guid.NewGuid();

        db.Set<VenueRentalRequest>().Add(
            new VenueRentalRequest
            {
                OrganizerUserId = Guid.NewGuid(),
                VenueId = venueId,
                StartAtUtc =
                    new DateTime(
                        2026, 9, 10, 10, 0, 0,
                        DateTimeKind.Utc),
                EndAtUtc =
                    new DateTime(
                        2026, 9, 10, 12, 0, 0,
                        DateTimeKind.Utc),
                Purpose = "Event A",
                OfferedAmount = 1000m,
                Status = RentalRequestStatus.Accepted
            });

        await db.SaveChangesAsync();

        var repository =
            new VenueRentalRepository(db);

        var hasOverlap =
            await repository.HasAcceptedOverlapAsync(
                venueId,
                new DateTime(
                    2026, 9, 10, 11, 0, 0,
                    DateTimeKind.Utc),
                new DateTime(
                    2026, 9, 10, 13, 0, 0,
                    DateTimeKind.Utc));

        Assert.True(hasOverlap);
    }

    [Fact]
    public async Task Adjacent_rentals_do_not_overlap()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<SEVPMSDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var db =
            new SEVPMSDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var venueId = Guid.NewGuid();

        db.Set<VenueRentalRequest>().Add(
            new VenueRentalRequest
            {
                OrganizerUserId = Guid.NewGuid(),
                VenueId = venueId,
                StartAtUtc =
                    new DateTime(
                        2026, 9, 10, 10, 0, 0,
                        DateTimeKind.Utc),
                EndAtUtc =
                    new DateTime(
                        2026, 9, 10, 12, 0, 0,
                        DateTimeKind.Utc),
                Purpose = "Event A",
                OfferedAmount = 1000m,
                Status = RentalRequestStatus.Accepted
            });

        await db.SaveChangesAsync();

        var repository =
            new VenueRentalRepository(db);

        var hasOverlap =
            await repository.HasAcceptedOverlapAsync(
                venueId,
                new DateTime(
                    2026, 9, 10, 12, 0, 0,
                    DateTimeKind.Utc),
                new DateTime(
                    2026, 9, 10, 14, 0, 0,
                    DateTimeKind.Utc));

        Assert.False(hasOverlap);
    }
}
