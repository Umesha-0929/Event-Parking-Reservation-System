using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class PaymentTransactionRepository(SEVPMSDbContext dbContext) : IPaymentTransactionRepository
{
    public async Task<IReadOnlyList<PaymentTransaction>> GetByPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<PaymentTransaction>()
            .AsNoTracking()
            .Where(x => x.PaymentId == paymentId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default)
        => await dbContext.Set<PaymentTransaction>().AddAsync(transaction, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
