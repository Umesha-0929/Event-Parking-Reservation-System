using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IPaymentTransactionRepository
{
    Task<IReadOnlyList<PaymentTransaction>> GetByPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
