using SEVPMS.Application.Features.EventCategories.DTOs;
using SEVPMS.Application.Features.EventCategories.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Events;

namespace SEVPMS.Application.Features.EventCategories.Services;

public sealed class EventCategoryService(IEventCategoryRepository repository) : IEventCategoryService
{
    public async Task<IReadOnlyList<EventCategoryResponse>> GetAsync(
        bool includeInactive,
        CancellationToken cancellationToken = default)
        => (await repository.GetAsync(includeInactive, cancellationToken)).Select(Map).ToList();

    public async Task<EventCategoryResponse> CreateAsync(
        UpsertEventCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var category = new EventCategory
        {
            Name = request.Name.Trim(),
            Code = NormalizeCode(request.Code),
            IsActive = request.IsActive
        };
        await repository.AddAsync(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<EventCategoryResponse> UpdateAsync(
        Guid id,
        UpsertEventCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);
        var category = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Event category was not found.");

        category.Name = request.Name.Trim();
        category.Code = NormalizeCode(request.Code);
        category.IsActive = request.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Event category was not found.");

        category.IsActive = false;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(UpsertEventCategoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Category name is required.");
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Category code is required.");
    }

    private static string NormalizeCode(string value)
        => value.Trim().ToUpperInvariant().Replace(' ', '-');

    private static EventCategoryResponse Map(EventCategory x) => new()
    {
        EventCategoryId = x.Id,
        Name = x.Name,
        Code = x.Code,
        IsActive = x.IsActive
    };
}
