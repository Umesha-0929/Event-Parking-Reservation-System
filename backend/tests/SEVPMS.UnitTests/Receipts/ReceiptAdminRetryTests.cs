using SEVPMS.Application.Features.Audit.DTOs;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Features.Receipts.Services;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Receipts;
using SEVPMS.Domain.Entities.Users;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Receipts;

public sealed class ReceiptAdminRetryTests
{
    [Fact]
    public async Task Admin_retry_retries_failed_channel_skips_sent_channel_and_audits()
    {
        var adminId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            ReceiptNumber = "RCT-TEST-001",
            PaymentId = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            CustomerUserId = customerId,
            Amount = 1500m,
            Currency = "LKR",
            IssuedAtUtc = DateTime.UtcNow
        };

        var smsDelivery = new ReceiptDelivery
        {
            Id = Guid.NewGuid(),
            ReceiptId = receipt.Id,
            CustomerUserId = customerId,
            Channel = "SMS",
            DestinationMasked = "*******1234",
            Status = ReceiptDeliveryStatus.Failed,
            AttemptCount = 1,
            LastError = "Temporary failure"
        };

        var emailDelivery = new ReceiptDelivery
        {
            Id = Guid.NewGuid(),
            ReceiptId = receipt.Id,
            CustomerUserId = customerId,
            Channel = "Email",
            DestinationMasked = "a***@example.com",
            Status = ReceiptDeliveryStatus.Sent,
            AttemptCount = 1,
            SentAtUtc = DateTime.UtcNow
        };

        var deliveryRepository =
            new FakeReceiptDeliveryRepository(
                smsDelivery,
                emailDelivery);

        var smsSender = new FakeSmsSender();
        var emailSender = new FakeEmailSender();
        var audit = new FakeAuditLogService();

        var service = new ReceiptDeliveryService(
            deliveryRepository,
            new FakeReceiptRepository(receipt),
            new FakeUserRepository(
                new User
                {
                    Id = customerId,
                    FirstName = "Test",
                    LastName = "Customer",
                    Email = "abc@example.com",
                    NormalizedEmail = "ABC@EXAMPLE.COM",
                    PasswordHash = "not-used",
                    PhoneNumber = "0771231234",
                    Role = UserRole.Customer,
                    Status = AccountStatus.Active
                }),
            smsSender,
            emailSender,
            audit);

        var result =
            await service.RetryForAdminAsync(
                adminId,
                receipt.Id);

        Assert.Equal(1, smsSender.CallCount);

        // Already-sent email must not be resent.
        Assert.Equal(0, emailSender.CallCount);

        var sms =
            Assert.Single(
                result.Where(
                    x => x.Channel == "SMS"));

        Assert.Equal(
            ReceiptDeliveryStatus.Sent,
            sms.Status);

        Assert.Equal(
            2,
            sms.AttemptCount);

        var email =
            Assert.Single(
                result.Where(
                    x => x.Channel == "Email"));

        Assert.Equal(
            ReceiptDeliveryStatus.Sent,
            email.Status);

        Assert.Equal(
            1,
            email.AttemptCount);

        var auditEntry =
            Assert.Single(audit.Entries);

        Assert.Equal(
            adminId,
            auditEntry.ActorUserId);

        Assert.Equal(
            "Receipt delivery retried by admin",
            auditEntry.Action);

        Assert.Equal(
            "Receipt",
            auditEntry.EntityType);

        Assert.Equal(
            receipt.Id.ToString(),
            auditEntry.EntityId);

        Assert.Contains(
            "SMS=Failed",
            auditEntry.BeforeSummary ?? string.Empty);

        Assert.Contains(
            "SMS=Sent",
            auditEntry.AfterSummary ?? string.Empty);
    }

    private sealed class FakeReceiptDeliveryRepository
        : IReceiptDeliveryRepository
    {
        private readonly List<ReceiptDelivery> deliveries;

        public FakeReceiptDeliveryRepository(
            params ReceiptDelivery[] deliveries)
        {
            this.deliveries =
                deliveries.ToList();
        }

        public Task<IReadOnlyList<ReceiptDelivery>> GetByReceiptAsync(
            Guid receiptId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ReceiptDelivery>>(
                deliveries
                    .Where(x => x.ReceiptId == receiptId)
                    .ToList());
        }

        public Task<ReceiptDelivery?> GetByReceiptAndChannelAsync(
            Guid receiptId,
            string channel,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                deliveries.FirstOrDefault(
                    x =>
                        x.ReceiptId == receiptId &&
                        string.Equals(
                            x.Channel,
                            channel,
                            StringComparison.OrdinalIgnoreCase)));
        }

        public Task AddAsync(
            ReceiptDelivery delivery,
            CancellationToken cancellationToken = default)
        {
            deliveries.Add(delivery);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeReceiptRepository(
        Receipt receipt)
        : IReceiptRepository
    {
        public Task<Receipt?> GetByIdAsync(
            Guid receiptId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Receipt?>(
                receipt.Id == receiptId
                    ? receipt
                    : null);
        }

        public Task<Receipt?> GetByPaymentIdAsync(
            Guid paymentId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Receipt?>(
                receipt.PaymentId == paymentId
                    ? receipt
                    : null);
        }

        public Task<IReadOnlyList<Receipt>> GetByCustomerAsync(
            Guid customerUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Receipt>>(
                receipt.CustomerUserId == customerUserId
                    ? new[] { receipt }
                    : Array.Empty<Receipt>());
        }

        public Task AddAsync(
            Receipt value,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository(
        User user)
        : IUserRepository
    {
        public Task<IReadOnlyList<User>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<User>>(
                new[] { user });
        }

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(
                user.Id == userId
                    ? user
                    : null);
        }

        public Task<User?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(
                user.NormalizedEmail == normalizedEmail
                    ? user
                    : null);
        }

        public Task<RefreshToken?> GetByRefreshTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<RefreshToken?>(null);
        }

        public Task<PasswordResetToken?> GetPasswordResetTokenByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PasswordResetToken?>(null);
        }

        public Task AddAsync(
            User value,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task AddRefreshTokenAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task AddPasswordResetTokenAsync(
            PasswordResetToken token,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RevokeActiveRefreshTokensAsync(
            Guid userId,
            DateTime revokedAtUtc,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task InvalidatePasswordResetTokensAsync(
            Guid userId,
            DateTime usedAtUtc,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSmsSender
        : ISmsSender
    {
        public int CallCount { get; private set; }

        public Task SendAsync(
            string phoneNumber,
            string message,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmailSender
        : IEmailSender
    {
        public int CallCount { get; private set; }

        public Task SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditLogService
        : IAuditLogService
    {
        public List<AuditEntry> Entries { get; } = new();

        public Task WriteAsync(
            Guid? actorUserId,
            string action,
            string entityType,
            string? entityId,
            string? beforeSummary,
            string? afterSummary,
            string? correlationId,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            Entries.Add(
                new AuditEntry(
                    actorUserId,
                    action,
                    entityType,
                    entityId,
                    beforeSummary,
                    afterSummary));

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AuditLogResponse>> QueryAsync(
            AuditLogQuery query,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<AuditLogResponse>>(
                Array.Empty<AuditLogResponse>());
        }
    }

    private sealed record AuditEntry(
        Guid? ActorUserId,
        string Action,
        string EntityType,
        string? EntityId,
        string? BeforeSummary,
        string? AfterSummary);
}