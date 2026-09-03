using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.EventCategories.DTOs;
using SEVPMS.Application.Features.EventCategories.Interfaces;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/event-categories")]
public sealed class EventCategoriesController(IEventCategoryService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventCategoryResponse>>> Get(CancellationToken cancellationToken)
        => Ok(await service.GetAsync(false, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpGet("admin")]
    public async Task<ActionResult<IReadOnlyList<EventCategoryResponse>>> GetAdmin(CancellationToken cancellationToken)
        => Ok(await service.GetAsync(true, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost]
    public async Task<ActionResult<EventCategoryResponse>> Create(
        [FromBody] UpsertEventCategoryRequest request,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await service.CreateAsync(request, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventCategoryResponse>> Update(
        Guid id,
        [FromBody] UpsertEventCategoryRequest request,
        CancellationToken cancellationToken)
        => Ok(await service.UpdateAsync(id, request, cancellationToken));

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
