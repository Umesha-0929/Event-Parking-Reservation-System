using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Controllers;
using SEVPMS.Application.Features.Food.DTOs;
using SEVPMS.Application.Features.Food.Interfaces;
using SEVPMS.Application.Features.Food.Validators;
using Xunit;

namespace SEVPMS.IntegrationTests.Food;

public sealed class FoodControllerTests
{
    [Fact]
    public async Task GetEventStalls_ReturnsOk()
    {
        var service = new FakeFoodService
        {
            Stalls =
            [
                new EventFoodStallDto
                {
                    Id = Guid.NewGuid(),
                    EventId = Guid.NewGuid(),
                    VendorId = Guid.NewGuid(),
                    StallName = "Main Food Stall",
                    IsActive = true
                }
            ]
        };

        var controller = CreateController(service);

        var result = await controller.GetEventStalls(
            Guid.NewGuid(),
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var stalls =
            Assert.IsAssignableFrom<IReadOnlyList<EventFoodStallDto>>(
                okResult.Value);

        Assert.Single(stalls);
    }

    [Fact]
    public async Task GetMenu_WhenStallNotFound_ReturnsNotFound()
    {
        var service = new FakeFoodService
        {
            ThrowMenuNotFound = true
        };

        var controller = CreateController(service);

        var result = await controller.GetMenu(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetMyOrders_WhenUserClaimMissing_ReturnsUnauthorized()
    {
        var service = new FakeFoodService();

        var controller = CreateController(service);

        var result = await controller.GetMyOrders(
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetMyOrders_WhenAuthenticated_ReturnsOrders()
    {
        var userId = Guid.NewGuid();

        var service = new FakeFoodService
        {
            Orders =
            [
                new FoodOrderDto
                {
                    Id = Guid.NewGuid(),
                    OrderNo = "FO-001",
                    CustomerUserId = userId,
                    EventId = Guid.NewGuid(),
                    EventFoodStallId = Guid.NewGuid(),
                    Status = "Pending",
                    FulfillmentType = "Pickup",
                    Total = 2500m,
                    CreatedAtUtc = DateTime.UtcNow
                }
            ]
        };

        var controller =
            CreateController(service, userId);

        var result = await controller.GetMyOrders(
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var orders =
            Assert.IsAssignableFrom<IReadOnlyList<FoodOrderDto>>(
                okResult.Value);

        Assert.Single(orders);
    }

    [Fact]
    public async Task GetOrderById_WhenOrderNotFound_ReturnsNotFound()
    {
        var service = new FakeFoodService
        {
            ThrowOrderNotFound = true
        };

        var controller =
            CreateController(
                service,
                Guid.NewGuid());

        var result = await controller.GetOrderById(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ReturnsCreatedAtAction()
    {
        var userId = Guid.NewGuid();

        var createdOrder = new FoodOrderDto
        {
            Id = Guid.NewGuid(),
            OrderNo = "FO-002",
            CustomerUserId = userId,
            EventId = Guid.NewGuid(),
            EventFoodStallId = Guid.NewGuid(),
            Status = "Pending",
            FulfillmentType = "Pickup",
            Total = 1500m,
            CreatedAtUtc = DateTime.UtcNow
        };

        var service = new FakeFoodService
        {
            CreateResult = createdOrder
        };

        var controller =
            CreateController(service, userId);

        var request = new CreateFoodOrderRequest
        {
            EventId = createdOrder.EventId,
            EventFoodStallId =
                createdOrder.EventFoodStallId,
            FulfillmentType = "Pickup",
            Items =
            [
                new CreateFoodOrderItemRequest
                {
                    MenuItemId = Guid.NewGuid(),
                    Quantity = 1
                }
            ]
        };

        var result = await controller.CreateOrder(
            request,
            CancellationToken.None);

        var createdResult =
            Assert.IsType<CreatedAtActionResult>(
                result.Result);

        Assert.Equal(
            nameof(FoodController.GetOrderById),
            createdResult.ActionName);

        Assert.Equal(
            createdOrder,
            createdResult.Value);
    }

    [Fact]
    public async Task CreateOrder_WhenValidationFails_ReturnsBadRequest()
    {
        var service = new FakeFoodService
        {
            ThrowValidationException = true
        };

        var controller =
            CreateController(
                service,
                Guid.NewGuid());

        var request = new CreateFoodOrderRequest
        {
            EventId = Guid.NewGuid(),
            EventFoodStallId = Guid.NewGuid(),
            FulfillmentType = "Pickup",
            Items =
            [
                new CreateFoodOrderItemRequest
                {
                    MenuItemId = Guid.NewGuid(),
                    Quantity = 1
                }
            ]
        };

        var result = await controller.CreateOrder(
            request,
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(
            result.Result);
    }

    private static FoodController CreateController(
        IFoodService service,
        Guid? userId = null)
    {
        var controller =
            new FoodController(service);

        var identity =
            new ClaimsIdentity();

        if (userId.HasValue)
        {
            identity.AddClaim(
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.Value.ToString()));
        }

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User =
                            new ClaimsPrincipal(
                                identity)
                    }
            };

        return controller;
    }

    private sealed class FakeFoodService
        : IFoodService
    {
        public IReadOnlyList<EventFoodStallDto> Stalls { get; init; }
            = [];

        public IReadOnlyList<EventMenuItemDto> MenuItems { get; init; }
            = [];

        public IReadOnlyList<FoodOrderDto> Orders { get; init; }
            = [];

        public IReadOnlyList<FoodOrderStatusHistoryDto> History { get; init; }
            = [];

        public FoodOrderDto? CreateResult { get; init; }

        public bool ThrowMenuNotFound { get; init; }

        public bool ThrowOrderNotFound { get; init; }

        public bool ThrowValidationException { get; init; }

        public Task<IReadOnlyList<EventFoodStallDto>>
            GetActiveStallsByEventAsync(
                Guid eventId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Stalls);
        }

        public Task<IReadOnlyList<EventMenuItemDto>>
            GetMenuByStallAsync(
                Guid eventFoodStallId,
                CancellationToken cancellationToken = default)
        {
            if (ThrowMenuNotFound)
            {
                throw new KeyNotFoundException();
            }

            return Task.FromResult(MenuItems);
        }

        public Task<FoodOrderDto> GetOrderByIdAsync(
            Guid customerUserId,
            Guid foodOrderId,
            CancellationToken cancellationToken = default)
        {
            if (ThrowOrderNotFound)
            {
                throw new KeyNotFoundException();
            }

            var order =
                Orders.FirstOrDefault(
                    x => x.Id == foodOrderId);

            if (order is null)
            {
                throw new KeyNotFoundException();
            }

            return Task.FromResult(order);
        }

        public Task<IReadOnlyList<FoodOrderDto>>
            GetOrdersByCustomerAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Orders);
        }

        public Task<IReadOnlyList<FoodOrderStatusHistoryDto>>
            GetOrderStatusHistoryAsync(
                Guid customerUserId,
                Guid foodOrderId,
                CancellationToken cancellationToken = default)
        {
            if (ThrowOrderNotFound)
            {
                throw new KeyNotFoundException();
            }

            return Task.FromResult(History);
        }

        public Task<FoodOrderDto> CreateOrderAsync(
            Guid customerUserId,
            CreateFoodOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (ThrowValidationException)
            {
                throw new FoodOrderValidationException(
                    "Food order is invalid.");
            }

            return Task.FromResult(
                CreateResult
                ?? throw new InvalidOperationException(
                    "CreateResult was not configured."));
        }

        public Task<FoodOrderDto> UpdateOrderStatusAsync(
            Guid changedByUserId,
            Guid foodOrderId,
            UpdateFoodOrderStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}