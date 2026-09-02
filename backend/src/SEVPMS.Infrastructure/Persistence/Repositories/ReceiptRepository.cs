using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class ReceiptRepository(SEVPMSDbContext dbContext) : IReceiptRepository
{
    public Task<Receipt?> GetByIdAsync(Guid receiptId, CancellationToken cancellationToken = default)
        => dbContext.Set<Receipt>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == receiptId, cancellationToken);

    public Task<Receipt?> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        => dbContext.Set<Receipt>().AsNoTracking().FirstOrDefaultAsync(x => x.PaymentId == paymentId, cancellationToken);

    public async Task<IReadOnlyList<Receipt>> GetByCustomerAsync(Guid customerUserId, CancellationToken cancellationToken = default)
        => await dbContext.Set<Receipt>()
            .AsNoTracking()
            .Where(x => x.CustomerUserId == customerUserId)
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default)
        => await dbContext.Set<Receipt>().AddAsync(receipt, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}
