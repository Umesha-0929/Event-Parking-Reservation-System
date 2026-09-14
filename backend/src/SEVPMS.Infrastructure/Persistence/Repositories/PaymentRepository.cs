using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Common.Paging;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(SEVPMSDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
        => dbContext.Set<Payment>()
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

    public Task<Payment?> GetByBookingIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
        => dbContext.Set<Payment>()
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.BookingId == bookingId, cancellationToken);

    public Task<Payment?> GetByCheckoutReferenceAsync(
        string checkoutReference,
        CancellationToken cancellationToken = default)
        => dbContext.Set<Payment>()
            .FirstOrDefaultAsync(
                x => x.CheckoutReference == checkoutReference,
                cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetByCustomerAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<Payment>()
            .AsNoTracking()
            .Where(x => x.CustomerUserId == customerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Payment>> GetByCustomerPageAsync(
        Guid customerUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (_, size, skip) = PagingRules.Normalize(page, pageSize);
        return await dbContext.Set<Payment>()
            .AsNoTracking()
            .Where(x => x.CustomerUserId == customerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(size)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetPendingManualAsync(
        CancellationToken cancellationToken = default)
        => await dbContext.Set<Payment>()
            .AsNoTracking()
            .Where(x => x.Provider == "OrganizerQr" && x.Status == SEVPMS.Domain.Enums.PaymentStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<Payment>().AddAsync(payment, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
