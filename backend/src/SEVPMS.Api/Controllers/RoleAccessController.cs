using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/role-access")]
[Authorize]
public sealed class RoleAccessController : ControllerBase
{
    [HttpGet("customer")]
    [Authorize(
        Policy =
            AuthorizationPolicies.CustomerOnly)]
    public IActionResult Customer()
    {
        return Ok(
            new
            {
                message =
                    "Customer access granted."
            });
    }

    [HttpGet("organizer")]
    [Authorize(
        Policy =
            AuthorizationPolicies.EventOrganizerOnly)]
    public IActionResult Organizer()
    {
        return Ok(
            new
            {
                message =
                    "Event Organizer access granted."
            });
    }

    [HttpGet("venue-owner")]
    [Authorize(
        Policy =
            AuthorizationPolicies.VenueOwnerOnly)]
    public IActionResult VenueOwner()
    {
        return Ok(
            new
            {
                message =
                    "Venue Owner access granted."
            });
    }

    [HttpGet("admin")]
    [Authorize(
        Policy =
            AuthorizationPolicies.AdminOnly)]
    public IActionResult Admin()
    {
        return Ok(
            new
            {
                message =
                    "Admin access granted."
            });
    }
}