using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Food.DTOs;
using SEVPMS.Application.Features.Food.Interfaces;
using SEVPMS.Application.Features.Food.Validators;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/food")]
public sealed class FoodController(
    IFoodService foodService)
    : ControllerBase
{
    [HttpGet("events/{eventId:guid}/stalls")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<EventFoodStallDto>>>
        GetEventStalls(
            Guid eventId,
            CancellationToken cancellationToken)
    {
        var stalls =
            await foodService.GetActiveStallsByEventAsync(
                eventId,
                cancellationToken);

        return Ok(stalls);
    }

    [HttpGet("stalls/{eventFoodStallId:guid}/menu")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<EventMenuItemDto>>>
        GetMenu(
            Guid eventFoodStallId,
            CancellationToken cancellationToken)
    {
        try
        {
            var menu =
                await foodService.GetMenuByStallAsync(
                    eventFoodStallId,
                    cancellationToken);

            return Ok(menu);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("orders")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<FoodOrderDto>>>
        GetMyOrders(
            CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var orders =
            await foodService.GetOrdersByCustomerAsync(
                userId,
                cancellationToken);

        return Ok(orders);
    }

    [HttpGet("orders/{foodOrderId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<FoodOrderDto>>
        GetOrderById(
            Guid foodOrderId,
            CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var order =
                await foodService.GetOrderByIdAsync(
                    userId,
                    foodOrderId,
                    cancellationToken);

            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("orders/{foodOrderId:guid}/history")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<FoodOrderStatusHistoryDto>>>
        GetOrderHistory(
            Guid foodOrderId,
            CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var history =
                await foodService.GetOrderStatusHistoryAsync(
                    userId,
                    foodOrderId,
                    cancellationToken);

            return Ok(history);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("orders")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<FoodOrderDto>>
        CreateOrder(
            CreateFoodOrderRequest request,
            CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var order =
                await foodService.CreateOrderAsync(
                    userId,
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetOrderById),
                new
                {
                    foodOrderId = order.Id
                },
                order);
        }
        catch (FoodOrderValidationException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    private bool TryGetUserId(
        out Guid userId)
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            value,
            out userId);
    }
}