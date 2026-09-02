using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(Guid receiptId, CancellationToken cancellationToken = default);
    Task<Receipt?> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Receipt>> GetByCustomerAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
