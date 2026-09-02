using SEVPMS.Application.Features.Receipts.DTOs;

namespace SEVPMS.Application.Features.Receipts.Interfaces;

public interface IReceiptService
{
    Task<IReadOnlyList<ReceiptResponse>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    Task<ReceiptResponse> GetByIdAsync(Guid customerUserId, Guid receiptId, CancellationToken cancellationToken = default);
    Task<ReceiptResponse> IssueAsync(Guid paymentId, Guid bookingId, Guid customerUserId, decimal amount, string currency, CancellationToken cancellationToken = default);
}
