using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Receipts.DTOs;
using SEVPMS.Application.Features.Receipts.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Application.Features.Receipts.Services;

public sealed class ReceiptService(
    IReceiptRepository receiptRepository,
    IReceiptDeliveryService? receiptDeliveryService = null)
    : IReceiptService
{
    public async Task<IReadOnlyList<ReceiptResponse>> GetMineAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default)
        => (await receiptRepository.GetByCustomerAsync(customerUserId, cancellationToken))
            .Select(Map)
            .ToList();

    public async Task<ReceiptResponse> GetByIdAsync(
        Guid customerUserId,
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        var receipt = await receiptRepository.GetByIdAsync(receiptId, cancellationToken)
            ?? throw new KeyNotFoundException("Receipt was not found.");

        if (receipt.CustomerUserId != customerUserId)
            throw new ForbiddenAccessException("You do not own this receipt.");

        return Map(receipt);
    }

    public async Task<ReceiptResponse> IssueAsync(
        Guid paymentId,
        Guid bookingId,
        Guid customerUserId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var existing = await receiptRepository.GetByPaymentIdAsync(paymentId, cancellationToken);

        if (existing is not null)
        {
            if (receiptDeliveryService is not null)
                await receiptDeliveryService.EnsureDeliveredAsync(existing, cancellationToken);
            return Map(existing);
        }

        var receipt = new Receipt
        {
            ReceiptNumber =
                $"RCT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            PaymentId = paymentId,
            BookingId = bookingId,
            CustomerUserId = customerUserId,
            Amount = amount,
            Currency = string.IsNullOrWhiteSpace(currency)
                ? "LKR"
                : currency.Trim().ToUpperInvariant(),
            IssuedAtUtc = DateTime.UtcNow
        };

        await receiptRepository.AddAsync(receipt, cancellationToken);
        await receiptRepository.SaveChangesAsync(cancellationToken);

        if (receiptDeliveryService is not null)
            await receiptDeliveryService.EnsureDeliveredAsync(receipt, cancellationToken);

        return Map(receipt);
    }

    private static ReceiptResponse Map(Receipt x) => new()
    {
        ReceiptId = x.Id,
        ReceiptNumber = x.ReceiptNumber,
        PaymentId = x.PaymentId,
        BookingId = x.BookingId,
        CustomerUserId = x.CustomerUserId,
        Amount = x.Amount,
        Currency = x.Currency,
        IssuedAtUtc = x.IssuedAtUtc
    };
}
