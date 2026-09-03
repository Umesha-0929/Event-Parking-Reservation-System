using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IRefundRepository
{
    Task<Refund?> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task AddAsync(Refund refund, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
