using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Audit.Interfaces;
using SEVPMS.Application.Features.Receipts.DTOs;
using SEVPMS.Application.Features.Receipts.Interfaces;
using SEVPMS.Application.Interfaces.Providers;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Receipts;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Receipts.Services;

public sealed class ReceiptDeliveryService(
    IReceiptDeliveryRepository deliveryRepository,
    IReceiptRepository receiptRepository,
    IUserRepository userRepository,
    ISmsSender smsSender,
    IEmailSender emailSender,
    IAuditLogService? auditLogService = null)
    : IReceiptDeliveryService
{
    public async Task EnsureDeliveredAsync(
        Receipt receipt,
        CancellationToken cancellationToken = default)
    {
        var user =
            await userRepository.GetByIdAsync(
                receipt.CustomerUserId,
                cancellationToken);

        if (user is null)
            return;

        if (!string.IsNullOrWhiteSpace(
                user.PhoneNumber))
        {
            await SendChannelAsync(
                receipt,
                "SMS",
                MaskPhone(user.PhoneNumber),
                ct => smsSender.SendAsync(
                    user.PhoneNumber,
                    BuildSms(receipt),
                    ct),
                cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(
                user.Email))
        {
            await SendChannelAsync(
                receipt,
                "Email",
                MaskEmail(user.Email),
                ct => emailSender.SendAsync(
                    user.Email,
                    $"SEVPMS receipt {receipt.ReceiptNumber}",
                    BuildEmail(receipt),
                    ct),
                cancellationToken);
        }
    }

    public async Task<IReadOnlyList<ReceiptDeliveryResponse>> GetAsync(
        Guid customerUserId,
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        var receipt =
            await receiptRepository.GetByIdAsync(
                receiptId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Receipt was not found.");

        if (receipt.CustomerUserId !=
            customerUserId)
        {
            throw new ForbiddenAccessException(
                "You do not own this receipt.");
        }

        return await GetResponsesAsync(
            receiptId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ReceiptDeliveryResponse>> RetryAsync(
        Guid customerUserId,
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        var receipt =
            await receiptRepository.GetByIdAsync(
                receiptId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Receipt was not found.");

        if (receipt.CustomerUserId !=
            customerUserId)
        {
            throw new ForbiddenAccessException(
                "You do not own this receipt.");
        }

        await EnsureDeliveredAsync(
            receipt,
            cancellationToken);

        return await GetResponsesAsync(
            receiptId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ReceiptDeliveryResponse>> GetForAdminAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        _ =
            await receiptRepository.GetByIdAsync(
                receiptId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Receipt was not found.");

        return await GetResponsesAsync(
            receiptId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ReceiptDeliveryResponse>> RetryForAdminAsync(
        Guid adminUserId,
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        if (adminUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Admin user is required.");
        }

        var receipt =
            await receiptRepository.GetByIdAsync(
                receiptId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Receipt was not found.");

        var before =
            await GetResponsesAsync(
                receiptId,
                cancellationToken);

        await EnsureDeliveredAsync(
            receipt,
            cancellationToken);

        var after =
            await GetResponsesAsync(
                receiptId,
                cancellationToken);

        if (auditLogService is not null)
        {
            await auditLogService.WriteAsync(
                adminUserId,
                "Receipt delivery retried by admin",
                "Receipt",
                receipt.Id.ToString(),
                BuildDeliverySummary(before),
                BuildDeliverySummary(after),
                null,
                null,
                cancellationToken);
        }

        return after;
    }

    private async Task<IReadOnlyList<ReceiptDeliveryResponse>> GetResponsesAsync(
        Guid receiptId,
        CancellationToken cancellationToken)
    {
        return (
            await deliveryRepository.GetByReceiptAsync(
                receiptId,
                cancellationToken))
            .Select(Map)
            .ToList();
    }

    private async Task SendChannelAsync(
        Receipt receipt,
        string channel,
        string maskedDestination,
        Func<CancellationToken, Task> send,
        CancellationToken cancellationToken)
    {
        var delivery =
            await deliveryRepository
                .GetByReceiptAndChannelAsync(
                    receipt.Id,
                    channel,
                    cancellationToken);

        if (delivery?.Status ==
            ReceiptDeliveryStatus.Sent)
        {
            return;
        }

        var isNew =
            delivery is null;

        delivery ??=
            new ReceiptDelivery
            {
                ReceiptId =
                    receipt.Id,

                CustomerUserId =
                    receipt.CustomerUserId,

                Channel =
                    channel,

                DestinationMasked =
                    maskedDestination,

                Status =
                    ReceiptDeliveryStatus.Pending
            };

        if (isNew)
        {
            await deliveryRepository.AddAsync(
                delivery,
                cancellationToken);
        }

        delivery.AttemptCount++;

        delivery.LastAttemptAtUtc =
            DateTime.UtcNow;

        delivery.UpdatedAtUtc =
            DateTime.UtcNow;

        try
        {
            await send(
                cancellationToken);

            delivery.Status =
                ReceiptDeliveryStatus.Sent;

            delivery.SentAtUtc =
                DateTime.UtcNow;

            delivery.LastError =
                null;
        }
        catch (Exception ex)
        {
            delivery.Status =
                ReceiptDeliveryStatus.Failed;

            delivery.LastError =
                ex.Message.Length > 900
                    ? ex.Message[..900]
                    : ex.Message;
        }

        await deliveryRepository.SaveChangesAsync(
            cancellationToken);
    }

    private static string BuildDeliverySummary(
        IReadOnlyList<ReceiptDeliveryResponse> deliveries)
    {
        if (deliveries.Count == 0)
            return "No delivery records";

        return string.Join(
            ", ",
            deliveries.Select(
                x =>
                    $"{x.Channel}={x.Status}, Attempts={x.AttemptCount}"));
    }

    private static string BuildSms(
        Receipt receipt)
    {
        return
            $"SEVPMS receipt {receipt.ReceiptNumber}: " +
            $"{receipt.Amount:0.00} {receipt.Currency}, " +
            $"{receipt.IssuedAtUtc:u}. " +
            $"View after sign-in: /api/receipts/{receipt.Id}";
    }

    private static string BuildEmail(
        Receipt receipt)
    {
        return
            $"Receipt: {receipt.ReceiptNumber}\n" +
            $"Amount: {receipt.Amount:0.00} {receipt.Currency}\n" +
            $"Issued: {receipt.IssuedAtUtc:u}\n" +
            $"Protected API link: /api/receipts/{receipt.Id}";
    }

    private static string MaskPhone(
        string value)
    {
        return value.Length <= 4
            ? "****"
            : new string('*', value.Length - 4)
              + value[^4..];
    }

    private static string MaskEmail(
        string value)
    {
        var at =
            value.IndexOf('@');

        return at <= 1
            ? "***"
            : value[0]
              + "***"
              + value[at..];
    }

    private static ReceiptDeliveryResponse Map(
        ReceiptDelivery delivery)
    {
        return new ReceiptDeliveryResponse
        {
            ReceiptDeliveryId =
                delivery.Id,

            ReceiptId =
                delivery.ReceiptId,

            Channel =
                delivery.Channel,

            DestinationMasked =
                delivery.DestinationMasked,

            Status =
                delivery.Status,

            AttemptCount =
                delivery.AttemptCount,

            LastAttemptAtUtc =
                delivery.LastAttemptAtUtc,

            SentAtUtc =
                delivery.SentAtUtc,

            LastError =
                delivery.LastError
        };
    }
}