using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Events.DTOs;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class EventRepository(
    SEVPMSDbContext dbContext)
    : IEventRepository
{
    public async Task<IReadOnlyList<Event>> GetPublishedAsync(
        EventSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query =
            dbContext.Set<Event>()
                .AsNoTracking()
                .Where(x =>
                    x.Status == EventStatus.Published);

        if (!string.IsNullOrWhiteSpace(
                request.Search))
        {
            var search =
                request.Search.Trim();

            query = query.Where(x =>
                x.Title.Contains(search) ||
                x.Description.Contains(search));
        }

        if (request.Venue.HasValue &&
            request.Venue.Value != Guid.Empty)
        {
            query = query.Where(x =>
                x.VenueId ==
                request.Venue.Value);
        }

        if (!string.IsNullOrWhiteSpace(
                request.Category))
        {
            var category =
                request.Category.Trim();

            query = query.Where(x =>
                x.Category == category);
        }

        if (request.Date.HasValue)
        {
            var startUtc =
                DateTime.SpecifyKind(
                    request.Date.Value
                        .ToDateTime(
                            TimeOnly.MinValue),
                    DateTimeKind.Utc);

            var endUtc =
                startUtc.AddDays(1);

            query = query.Where(x =>
                x.StartAtUtc >= startUtc &&
                x.StartAtUtc < endUtc);
        }

        return await query
            .OrderBy(x => x.StartAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>>
        GetByOrganizerUserIdAsync(
            Guid organizerUserId,
            CancellationToken cancellationToken = default)
        => await dbContext.Set<Event>()
            .AsNoTracking()
            .Where(x =>
                x.OrganizerUserId ==
                organizerUserId)
            .OrderByDescending(
                x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<Event?> GetByIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
        => dbContext.Set<Event>()
            .FirstOrDefaultAsync(
                x => x.Id == eventId,
                cancellationToken);

    public async Task AddAsync(
        Event eventEntity,
        CancellationToken cancellationToken = default)
        => await dbContext.Set<Event>()
            .AddAsync(
                eventEntity,
                cancellationToken);

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => await dbContext
            .SaveChangesAsync(
                cancellationToken);
}