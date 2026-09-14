using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByCheckoutReferenceAsync(string checkoutReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByCustomerAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<Payment>> GetByCustomerPageAsync(
        Guid customerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetByCustomerAsync(customerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<IReadOnlyList<Payment>> GetPendingManualAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
