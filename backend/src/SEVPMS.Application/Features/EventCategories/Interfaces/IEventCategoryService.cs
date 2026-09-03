using SEVPMS.Application.Features.EventCategories.DTOs;

namespace SEVPMS.Application.Features.EventCategories.Interfaces;

public interface IEventCategoryService
{
    Task<IReadOnlyList<EventCategoryResponse>> GetAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<EventCategoryResponse> CreateAsync(UpsertEventCategoryRequest request, CancellationToken cancellationToken = default);
    Task<EventCategoryResponse> UpdateAsync(Guid id, UpsertEventCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
