using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Domain.Entities.Seats;

namespace SEVPMS.Infrastructure.Persistence.Repositories.Seats;

public sealed class EfSeatingLayoutRepository : ISeatingLayoutRepository
{
    private readonly SEVPMSDbContext _db;

    public EfSeatingLayoutRepository(SEVPMSDbContext db)
    {
        _db = db;
    }

    public Task<SeatingLayout?> GetLayoutByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return _db.Set<SeatingLayout>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.EventId == eventId,
                cancellationToken);
    }

    public Task<SeatingLayout?> GetPublishedLayoutByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return _db.Set<SeatingLayout>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.EventId == eventId && x.IsPublished,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<SeatSection>> GetSectionsAsync(
        Guid seatingLayoutId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Set<SeatSection>()
            .AsNoTracking()
            .Where(x => x.SeatingLayoutId == seatingLayoutId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<SeatCategory>> GetCategoriesAsync(
        Guid seatingLayoutId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Set<SeatCategory>()
            .AsNoTracking()
            .Where(x => x.SeatingLayoutId == seatingLayoutId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Seat>> GetSeatsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Set<Seat>()
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.SectionId)
            .ThenBy(x => x.RowNumber)
            .ThenBy(x => x.ColumnNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<SeatingLayout> AddLayoutAsync(
        SeatingLayout layout,
        CancellationToken cancellationToken = default)
    {
        _db.Set<SeatingLayout>().Add(layout);

        await _db.SaveChangesAsync(cancellationToken);

        return layout;
    }

    public async Task UpdateLayoutAsync(
        SeatingLayout layout,
        CancellationToken cancellationToken = default)
    {
        layout.UpdatedAtUtc = DateTime.UtcNow;

        _db.Set<SeatingLayout>().Update(layout);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<SeatSection> UpsertSectionAsync(
        SeatSection section,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.Set<SeatSection>()
            .SingleOrDefaultAsync(
                x => x.Id == section.Id,
                cancellationToken);

        if (existing is null)
        {
            _db.Set<SeatSection>().Add(section);
        }
        else
        {
            existing.EventId = section.EventId;
            existing.SeatingLayoutId = section.SeatingLayoutId;
            existing.Name = section.Name;
            existing.Code = section.Code;
            existing.RowCount = section.RowCount;
            existing.ColumnCount = section.ColumnCount;
            existing.X = section.X;
            existing.Y = section.Y;
            existing.Width = section.Width;
            existing.Height = section.Height;
            existing.DisplayOrder = section.DisplayOrder;
            existing.IsAccessibleSection = section.IsAccessibleSection;
            existing.IsEnabled = section.IsEnabled;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return existing ?? section;
    }

    public async Task<SeatCategory> UpsertCategoryAsync(
        SeatCategory category,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.Set<SeatCategory>()
            .SingleOrDefaultAsync(
                x => x.Id == category.Id,
                cancellationToken);

        if (existing is null)
        {
            _db.Set<SeatCategory>().Add(category);
        }
        else
        {
            existing.EventId = category.EventId;
            existing.SeatingLayoutId = category.SeatingLayoutId;
            existing.Name = category.Name;
            existing.Code = category.Code;
            existing.Price = category.Price;
            existing.DisplayOrder = category.DisplayOrder;
            existing.IsActive = category.IsActive;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return existing ?? category;
    }

    public async Task ReplaceSectionSeatsAsync(
        Guid eventId,
        Guid sectionId,
        IReadOnlyCollection<Seat> seats,
        CancellationToken cancellationToken = default)
    {
        var existingSeats = await _db.Set<Seat>()
            .Where(x =>
                x.EventId == eventId &&
                x.SectionId == sectionId)
            .ToListAsync(cancellationToken);

        if (existingSeats.Count > 0)
        {
            _db.Set<Seat>().RemoveRange(existingSeats);
        }

        if (seats.Count > 0)
        {
            _db.Set<Seat>().AddRange(seats);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}
