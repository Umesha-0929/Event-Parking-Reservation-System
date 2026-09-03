using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class RefundRepository(SEVPMSDbContext dbContext) : IRefundRepository
{
    public Task<Refund?> GetByBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
        => dbContext.Set<Refund>().FirstOrDefaultAsync(x => x.BookingId == bookingId, cancellationToken);

    public async Task AddAsync(Refund refund, CancellationToken cancellationToken = default)
        => await dbContext.Set<Refund>().AddAsync(refund, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
