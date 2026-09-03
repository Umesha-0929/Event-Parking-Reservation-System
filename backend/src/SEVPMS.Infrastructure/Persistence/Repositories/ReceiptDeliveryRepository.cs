using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class ReceiptDeliveryRepository(SEVPMSDbContext dbContext) : IReceiptDeliveryRepository
{
    public async Task<IReadOnlyList<ReceiptDelivery>> GetByReceiptAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<ReceiptDelivery>()
            .Where(x => x.ReceiptId == receiptId)
            .OrderBy(x => x.Channel)
            .ToListAsync(cancellationToken);

    public Task<ReceiptDelivery?> GetByReceiptAndChannelAsync(
        Guid receiptId,
        string channel,
        CancellationToken cancellationToken = default)
        => dbContext.Set<ReceiptDelivery>()
            .FirstOrDefaultAsync(
                x => x.ReceiptId == receiptId && x.Channel == channel,
                cancellationToken);

    public async Task AddAsync(ReceiptDelivery delivery, CancellationToken cancellationToken = default)
        => await dbContext.Set<ReceiptDelivery>().AddAsync(delivery, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
