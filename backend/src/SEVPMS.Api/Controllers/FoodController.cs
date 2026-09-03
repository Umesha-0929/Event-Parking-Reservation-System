using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Authorization;
using SEVPMS.Application.Features.Food.DTOs;
using SEVPMS.Application.Features.Food.Interfaces;
using SEVPMS.Application.Features.Food.Validators;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;

namespace SEVPMS.Api.Controllers;

[ApiController]
[Route("api/food")]
public sealed class FoodController : ControllerBase
{
    private readonly IFoodService _foodService;
    private readonly IFoodRepository? _foodRepository;
    private readonly IEventRepository? _eventRepository;
    private readonly INotificationService? _notificationService;

    public FoodController(IFoodService foodService)
    {
        _foodService = foodService;
    }

    public FoodController(
        IFoodService foodService,
        IFoodRepository foodRepository,
        IEventRepository eventRepository,
        INotificationService notificationService)
        : this(foodService)
    {
        _foodRepository = foodRepository;
        _eventRepository = eventRepository;
        _notificationService = notificationService;
    }

    [HttpGet("events/{eventId:guid}/stalls")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<EventFoodStallDto>>> GetEventStalls(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var stalls = await _foodService.GetActiveStallsByEventAsync(eventId, cancellationToken);
        return Ok(stalls);
    }

    [HttpGet("stalls/{eventFoodStallId:guid}/menu")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<EventMenuItemDto>>> GetMenu(
        Guid eventFoodStallId,
        CancellationToken cancellationToken)
    {
        try
        {
            var menu = await _foodService.GetMenuByStallAsync(eventFoodStallId, cancellationToken);
            return Ok(menu);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("orders")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<FoodOrderDto>>> GetMyOrders(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var orders = await _foodService.GetOrdersByCustomerAsync(userId, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("orders/{foodOrderId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<FoodOrderDto>> GetOrderById(
        Guid foodOrderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _foodService.GetOrderByIdAsync(userId, foodOrderId, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("orders/{foodOrderId:guid}/history")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<IReadOnlyList<FoodOrderStatusHistoryDto>>> GetOrderHistory(
        Guid foodOrderId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var history = await _foodService.GetOrderStatusHistoryAsync(userId, foodOrderId, cancellationToken);
            return Ok(history);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("orders")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    public async Task<ActionResult<FoodOrderDto>> CreateOrder(
        CreateFoodOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _foodService.CreateOrderAsync(userId, request, cancellationToken);
            await PublishNewOrderNotificationsAsync(order, cancellationToken);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { foodOrderId = order.Id },
                order);
        }
        catch (FoodOrderValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPatch("orders/{foodOrderId:guid}/status")]
    [Authorize]
    public async Task<ActionResult<FoodOrderDto>> UpdateOrderStatus(
        Guid foodOrderId,
        UpdateFoodOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        if (_foodRepository is null || _eventRepository is null)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        var order = await _foodRepository.GetOrderByIdAsync(foodOrderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        var stall = await _foodRepository.GetStallByIdAsync(order.EventFoodStallId, cancellationToken);
        if (stall is null)
        {
            return NotFound();
        }

        var vendor = await _foodRepository.GetVendorByIdAsync(stall.VendorId, cancellationToken);
        var eventEntity = await _eventRepository.GetByIdAsync(order.EventId, cancellationToken);

        var isAdmin = User.IsInRole("Admin");
        var isOrganizer = eventEntity?.OrganizerUserId == userId;
        var isVendorOwner = vendor?.OwnerUserId == userId;

        if (!isAdmin && !isOrganizer && !isVendorOwner)
        {
            return Forbid();
        }

        try
        {
            var updated = await _foodService.UpdateOrderStatusAsync(
                userId,
                foodOrderId,
                request,
                cancellationToken);

            if (_notificationService is not null)
            {
                await _notificationService.CreateAsync(
                    order.CustomerUserId,
                    "Food order updated",
                    $"Food order {order.OrderNo} is now {updated.Status}.",
                    "FoodOrder",
                    cancellationToken);
            }

            return Ok(updated);
        }
        catch (FoodOrderValidationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task PublishNewOrderNotificationsAsync(
        FoodOrderDto order,
        CancellationToken cancellationToken)
    {
        if (_foodRepository is null ||
            _eventRepository is null ||
            _notificationService is null)
        {
            return;
        }

        var eventEntity = await _eventRepository.GetByIdAsync(order.EventId, cancellationToken);
        if (eventEntity is not null)
        {
            await _notificationService.CreateAsync(
                eventEntity.OrganizerUserId,
                "New food order",
                $"Food order {order.OrderNo} was placed.",
                "FoodOrder",
                cancellationToken);
        }

        var stall = await _foodRepository.GetStallByIdAsync(order.EventFoodStallId, cancellationToken);
        if (stall is null)
        {
            return;
        }

        var vendor = await _foodRepository.GetVendorByIdAsync(stall.VendorId, cancellationToken);
        if (vendor?.OwnerUserId is Guid vendorOwnerUserId &&
            vendorOwnerUserId != eventEntity?.OrganizerUserId)
        {
            await _notificationService.CreateAsync(
                vendorOwnerUserId,
                "New food order",
                $"Food order {order.OrderNo} was placed for {stall.StallName}.",
                "FoodOrder",
                cancellationToken);
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }
}
