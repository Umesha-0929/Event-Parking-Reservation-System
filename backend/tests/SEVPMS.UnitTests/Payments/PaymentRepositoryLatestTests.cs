using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SEVPMS.Domain.Entities.Payments;
using SEVPMS.Domain.Enums;
using SEVPMS.Infrastructure.Persistence;
using SEVPMS.Infrastructure.Persistence.Repositories;

namespace SEVPMS.UnitTests.Payments;

public sealed class PaymentRepositoryLatestTests
{
    [Fact]
    public async Task GetByBookingId_returns_latest_payment_attempt()
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

        var bookingId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var oldPayment = new Payment
        {
            BookingId = bookingId,
            CustomerUserId = customerId,
            Amount = 5000m,
            Currency = "LKR",
            Provider = "Mock",
            CheckoutReference = $"OLD-{Guid.NewGuid():N}",
            Status = PaymentStatus.Failed,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
        };

        var latestPayment = new Payment
        {
            BookingId = bookingId,
            CustomerUserId = customerId,
            Amount = 5000m,
            Currency = "LKR",
            Provider = "Mock",
            CheckoutReference = $"NEW-{Guid.NewGuid():N}",
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Set<Payment>().AddRange(
            oldPayment,
            latestPayment);

        await db.SaveChangesAsync();

        var repository =
            new PaymentRepository(db);

        var result =
            await repository.GetByBookingIdAsync(
                bookingId);

        Assert.NotNull(result);
        Assert.Equal(
            latestPayment.Id,
            result!.Id);
    }
}
