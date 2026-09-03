using SEVPMS.Domain.Entities.Receipts;

namespace SEVPMS.Application.Interfaces.Repositories;

public interface IReceiptDeliveryRepository
{
    Task<IReadOnlyList<ReceiptDelivery>> GetByReceiptAsync(Guid receiptId, CancellationToken cancellationToken = default);
    Task<ReceiptDelivery?> GetByReceiptAndChannelAsync(Guid receiptId, string channel, CancellationToken cancellationToken = default);
    Task AddAsync(ReceiptDelivery delivery, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
