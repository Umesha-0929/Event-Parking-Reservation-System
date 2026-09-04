using SEVPMS.Application.Features.Audit.DTOs;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Features.Auth.DTOs;
using SEVPMS.Application.Features.Auth.Interfaces;
using SEVPMS.Application.Features.Auth.Services;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Users;
using Xunit;

namespace SEVPMS.UnitTests.Auth;

public sealed class AccountSecurityAuditTests
{
    [Fact]
    public async Task Logout_all_sessions_writes_semantic_audit_log()
    {
        var userId = Guid.NewGuid();

        var userRepository =
            new FakeUserRepository();

        var audit =
            new FakeAuditLogService();

        var service =
            new AccountSecurityService(
                userRepository,
                new FakePasswordHasher(),
                new FakeRefreshTokenService(),
                new FakeEmailSender(),
                audit);

        await service.LogoutAsync(
            userId,
            new LogoutRequest
            {
                AllSessions = true
            });

        Assert.True(
            userRepository.RevokeActiveTokensCalled);

        Assert.Equal(
            userId,
            userRepository.RevokedUserId);

        Assert.Single(audit.Entries);

        var entry = audit.Entries[0];

        Assert.Equal(
            userId,
            entry.ActorUserId);

        Assert.Equal(
            "User logged out all sessions",
            entry.Action);

        Assert.Equal(
            "User",
            entry.EntityType);

        Assert.Equal(
            userId.ToString(),
            entry.EntityId);

        Assert.Equal(
            "All refresh tokens revoked",
            entry.AfterSummary);
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

        public Task<IReadOnlyList<AuditLogResponse>>
            QueryAsync(
                AuditLogQuery query,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<AuditLogResponse>>(
                    Array.Empty<AuditLogResponse>());
    }

    private sealed record AuditEntry(
        Guid? ActorUserId,
        string Action,
        string EntityType,
        string? EntityId,
        string? BeforeSummary,
        string? AfterSummary);

    private sealed class FakeUserRepository
        : IUserRepository
    {
        public bool RevokeActiveTokensCalled
        {
            get;
            private set;
        }

        public Guid? RevokedUserId
        {
            get;
            private set;
        }

        public Task<IReadOnlyList<User>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<User>>(
                Array.Empty<User>());

        public Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(null);

        public Task<User?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(null);

        public Task<RefreshToken?> GetByRefreshTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
            => Task.FromResult<RefreshToken?>(null);

        public Task<PasswordResetToken?>
            GetPasswordResetTokenByHashAsync(
                string tokenHash,
                CancellationToken cancellationToken = default)
            => Task.FromResult<PasswordResetToken?>(null);

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddRefreshTokenAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddPasswordResetTokenAsync(
            PasswordResetToken token,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RevokeActiveRefreshTokensAsync(
            Guid userId,
            DateTime revokedAtUtc,
            CancellationToken cancellationToken = default)
        {
            RevokeActiveTokensCalled = true;
            RevokedUserId = userId;

            return Task.CompletedTask;
        }

        public Task InvalidatePasswordResetTokensAsync(
            Guid userId,
            DateTime usedAtUtc,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakePasswordHasher
        : IPasswordHasher
    {
        public string HashPassword(
            string password)
            => $"HASH-{password}";

        public bool VerifyPassword(
            string passwordHash,
            string providedPassword)
            => true;
    }

    private sealed class FakeRefreshTokenService
        : IRefreshTokenService
    {
        public RefreshTokenResult GenerateToken()
            => new(
                "TOKEN",
                "HASH",
                DateTime.UtcNow.AddDays(7));

        public string HashToken(
            string token)
            => $"HASH-{token}";
    }

    private sealed class FakeEmailSender
        : IEmailSender
    {
        public Task SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}