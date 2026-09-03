using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class VenueRentalRepository(
    SEVPMSDbContext dbContext)
    : IVenueRentalRepository
{
    public Task<VenueRentalRequest?> GetByIdAsync(
        Guid rentalId,
        CancellationToken cancellationToken = default)
        => dbContext.Set<VenueRentalRequest>()
            .FirstOrDefaultAsync(
                x => x.Id == rentalId,
                cancellationToken);

    public async Task<IReadOnlyList<VenueRentalRequest>>
        GetByOrganizerAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueRentalRequest>()
            .AsNoTracking()
            .Where(x =>
                x.OrganizerUserId == organizerUserId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VenueRentalRequest>>
        GetByVenueIdsAsync(
            IReadOnlyCollection<Guid> venueIds,
            CancellationToken cancellationToken = default)
    {
        if (venueIds.Count == 0)
        {
            return Array.Empty<VenueRentalRequest>();
        }

        return await dbContext.Set<VenueRentalRequest>()
            .AsNoTracking()
            .Where(x =>
                venueIds.Contains(x.VenueId))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasAcceptedOverlapAsync(
        Guid venueId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        Guid? excludeRentalId = null,
        CancellationToken cancellationToken = default)
        => dbContext.Set<VenueRentalRequest>()
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.VenueId == venueId &&
                    x.Status ==
                        RentalRequestStatus.Accepted &&
                    (!excludeRentalId.HasValue ||
                     x.Id != excludeRentalId.Value) &&
                    x.StartAtUtc < endAtUtc &&
                    x.EndAtUtc > startAtUtc,
                cancellationToken);

    public async Task AddAsync(
        VenueRentalRequest request,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<VenueRentalRequest>()
            .AddAsync(
                request,
                cancellationToken);

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(
            cancellationToken);
}