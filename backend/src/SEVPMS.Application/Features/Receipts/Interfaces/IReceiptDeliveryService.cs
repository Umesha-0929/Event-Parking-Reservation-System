using SEVPMS.Application.Features.Receipts.DTOs;
using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Application.Features.Receipts.Interfaces;

public interface IReceiptDeliveryService
{
    Task EnsureDeliveredAsync(
        Receipt receipt,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReceiptDeliveryResponse>> GetAsync(
        Guid customerUserId,
        Guid receiptId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReceiptDeliveryResponse>> RetryAsync(
        Guid customerUserId,
        Guid receiptId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReceiptDeliveryResponse>> GetForAdminAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReceiptDeliveryResponse>> RetryForAdminAsync(
        Guid adminUserId,
        Guid receiptId,
        CancellationToken cancellationToken = default);
}