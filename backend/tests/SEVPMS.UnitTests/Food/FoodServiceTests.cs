using SEVPMS.Application.Features.Food.DTOs;
using SEVPMS.Application.Features.Food.Interfaces;
using SEVPMS.Application.Features.Food.Services;
using SEVPMS.Application.Features.Food.Validators;
using SEVPMS.Domain.Entities.Food;
using Xunit;

namespace SEVPMS.UnitTests.Food;

public sealed class FoodServiceTests
{
    [Fact]
    public async Task GetMenuByStallAsync_UsesEventPriceOverride()
    {
        var repository = new FakeFoodRepository();

        var vendorId = Guid.NewGuid();

        var stall = new EventFoodStall
        {
            EventId = Guid.NewGuid(),
            VendorId = vendorId,
            StallName = "Main Stall",
            IsActive = true,
            OpensAtUtc = DateTime.UtcNow.AddHours(-1),
            ClosesAtUtc = DateTime.UtcNow.AddHours(1)
        };

        var menuItem = new MenuItem
        {
            VendorId = vendorId,
            Name = "Burger",
            Description = "Cheese burger",
            Price = 1200m,
            Currency = "LKR",
            IsAvailable = true,
            ImageUrl = "burger.jpg"
        };

        var eventMenuItem = new EventMenuItem
        {
            EventFoodStallId = stall.Id,
            MenuItemId = menuItem.Id,
            EventPriceOverride = 1000m,
            IsAvailable = true
        };

        repository.Stalls.Add(stall);
        repository.MenuItems.Add(menuItem);
        repository.EventMenuItems.Add(eventMenuItem);

        var service = new FoodService(repository);

        var result =
            await service.GetMenuByStallAsync(stall.Id);

        var item = Assert.Single(result);

        Assert.Equal(menuItem.Id, item.MenuItemId);
        Assert.Equal("Burger", item.Name);
        Assert.Equal(1000m, item.Price);
        Assert.Equal("LKR", item.Currency);
    }

    [Fact]
    public async Task CreateOrderAsync_WithValidRequest_CalculatesTotalAndSavesOrder()
    {
        var repository = new FakeFoodRepository();

        var customerUserId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var vendorId = Guid.NewGuid();

        var stall = new EventFoodStall
        {
            EventId = eventId,
            VendorId = vendorId,
            StallName = "Food Zone",
            IsActive = true,
            OpensAtUtc = DateTime.UtcNow.AddHours(-1),
            ClosesAtUtc = DateTime.UtcNow.AddHours(1)
        };

        var menuItem = new MenuItem
        {
            VendorId = vendorId,
            Name = "Pizza",
            Description = "Large pizza",
            Price = 1500m,
            Currency = "LKR",
            IsAvailable = true,
            ImageUrl = "pizza.jpg"
        };

        var eventMenuItem = new EventMenuItem
        {
            EventFoodStallId = stall.Id,
            MenuItemId = menuItem.Id,
            EventPriceOverride = 1250m,
            IsAvailable = true
        };

        repository.Stalls.Add(stall);
        repository.MenuItems.Add(menuItem);
        repository.EventMenuItems.Add(eventMenuItem);

        var service = new FoodService(repository);

        var request = new CreateFoodOrderRequest
        {
            EventId = eventId,
            EventFoodStallId = stall.Id,
            FulfillmentType = "Pickup",
            Items =
            [
                new CreateFoodOrderItemRequest
                {
                    MenuItemId = menuItem.Id,
                    Quantity = 2
                }
            ]
        };

        var result =
            await service.CreateOrderAsync(
                customerUserId,
                request);

        Assert.Equal(customerUserId, result.CustomerUserId);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(stall.Id, result.EventFoodStallId);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(2500m, result.Total);

        Assert.Single(repository.Orders);
        Assert.Single(repository.OrderItems);
        Assert.Single(repository.StatusHistory);

        Assert.Equal(2500m, repository.OrderItems[0].LineTotal);
        Assert.Equal("Pending", repository.StatusHistory[0].NewStatus);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateOrderAsync_WhenStallBelongsToDifferentEvent_ThrowsValidationException()
    {
        var repository = new FakeFoodRepository();

        var stall = new EventFoodStall
        {
            EventId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            StallName = "Wrong Event Stall",
            IsActive = true,
            OpensAtUtc = DateTime.UtcNow.AddHours(-1),
            ClosesAtUtc = DateTime.UtcNow.AddHours(1)
        };

        repository.Stalls.Add(stall);

        var service = new FoodService(repository);

        var request = new CreateFoodOrderRequest
        {
            EventId = Guid.NewGuid(),
            EventFoodStallId = stall.Id,
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

        var exception =
            await Assert.ThrowsAsync<FoodOrderValidationException>(
                () => service.CreateOrderAsync(
                    Guid.NewGuid(),
                    request));

        Assert.Equal(
            "Food stall does not belong to the selected event.",
            exception.Message);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WhenOrderBelongsToAnotherCustomer_ThrowsKeyNotFoundException()
    {
        var repository = new FakeFoodRepository();

        var order = new FoodOrder
        {
            OrderNo = "FO-001",
            CustomerUserId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            EventFoodStallId = Guid.NewGuid(),
            Status = "Pending",
            FulfillmentType = "Pickup",
            Total = 1000m,
            CreatedAtUtc = DateTime.UtcNow
        };

        repository.Orders.Add(order);

        var service = new FoodService(repository);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetOrderByIdAsync(
                Guid.NewGuid(),
                order.Id));
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_WithValidTransition_UpdatesOrderAndHistory()
    {
        var repository = new FakeFoodRepository();

        var order = new FoodOrder
        {
            OrderNo = "FO-002",
            CustomerUserId = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            EventFoodStallId = Guid.NewGuid(),
            Status = "Pending",
            FulfillmentType = "Pickup",
            Total = 1800m,
            CreatedAtUtc = DateTime.UtcNow
        };

        repository.Orders.Add(order);

        var service = new FoodService(repository);

        var request = new UpdateFoodOrderStatusRequest
        {
            NewStatus = "Accepted",
            Note = "Vendor accepted order"
        };

        var changedByUserId = Guid.NewGuid();

        var result =
            await service.UpdateOrderStatusAsync(
                changedByUserId,
                order.Id,
                request);

        Assert.Equal("Accepted", result.Status);
        Assert.Equal("Accepted", order.Status);

        var history = Assert.Single(repository.StatusHistory);

        Assert.Equal("Pending", history.OldStatus);
        Assert.Equal("Accepted", history.NewStatus);
        Assert.Equal(changedByUserId, history.ChangedByUserId);
        Assert.Equal(
            "Vendor accepted order",
            history.Note);

        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    private sealed class FakeFoodRepository
        : IFoodRepository
    {
        public List<EventFoodStall> Stalls { get; } = [];
        public List<FoodVendor> Vendors { get; } = [];
        public List<MenuItem> MenuItems { get; } = [];
        public List<EventMenuItem> EventMenuItems { get; } = [];
        public List<FoodOrder> Orders { get; } = [];
        public List<FoodOrderItem> OrderItems { get; } = [];
        public List<FoodOrderStatusHistory> StatusHistory { get; } = [];

        public int SaveChangesCallCount { get; private set; }

        public Task<IReadOnlyList<EventFoodStall>> GetActiveStallsByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<EventFoodStall> result =
                Stalls
                    .Where(x =>
                        x.EventId == eventId &&
                        x.IsActive)
                    .ToList();

            return Task.FromResult(result);
        }

        public Task<EventFoodStall?> GetStallByIdAsync(
            Guid eventFoodStallId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Stalls.SingleOrDefault(
                    x => x.Id == eventFoodStallId));
        }

        public Task<FoodVendor?> GetVendorByIdAsync(
            Guid vendorId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Vendors.SingleOrDefault(
                    x => x.Id == vendorId));
        }

        public Task<IReadOnlyList<EventMenuItem>>
            GetAvailableEventMenuItemsByStallAsync(
                Guid eventFoodStallId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<EventMenuItem> result =
                EventMenuItems
                    .Where(x =>
                        x.EventFoodStallId == eventFoodStallId &&
                        x.IsAvailable)
                    .ToList();

            return Task.FromResult(result);
        }

        public Task<MenuItem?> GetMenuItemByIdAsync(
            Guid menuItemId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                MenuItems.SingleOrDefault(
                    x => x.Id == menuItemId));
        }

        public Task<EventMenuItem?> GetEventMenuItemAsync(
            Guid eventFoodStallId,
            Guid menuItemId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                EventMenuItems.SingleOrDefault(
                    x =>
                        x.EventFoodStallId == eventFoodStallId &&
                        x.MenuItemId == menuItemId));
        }

        public Task<FoodOrder?> GetOrderByIdAsync(
            Guid foodOrderId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Orders.SingleOrDefault(
                    x => x.Id == foodOrderId));
        }

        public Task<IReadOnlyList<FoodOrder>>
            GetOrdersByCustomerAsync(
                Guid customerUserId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FoodOrder> result =
                Orders
                    .Where(
                        x => x.CustomerUserId == customerUserId)
                    .ToList();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<FoodOrderItem>>
            GetOrderItemsAsync(
                Guid foodOrderId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FoodOrderItem> result =
                OrderItems
                    .Where(
                        x => x.FoodOrderId == foodOrderId)
                    .ToList();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<FoodOrderStatusHistory>>
            GetOrderStatusHistoryAsync(
                Guid foodOrderId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FoodOrderStatusHistory> result =
                StatusHistory
                    .Where(
                        x => x.FoodOrderId == foodOrderId)
                    .ToList();

            return Task.FromResult(result);
        }

        public Task AddOrderAsync(
            FoodOrder foodOrder,
            CancellationToken cancellationToken = default)
        {
            Orders.Add(foodOrder);

            return Task.CompletedTask;
        }

        public Task AddOrderItemsAsync(
            IReadOnlyCollection<FoodOrderItem> items,
            CancellationToken cancellationToken = default)
        {
            OrderItems.AddRange(items);

            return Task.CompletedTask;
        }

        public Task AddStatusHistoryAsync(
            FoodOrderStatusHistory history,
            CancellationToken cancellationToken = default)
        {
            StatusHistory.Add(history);

            return Task.CompletedTask;
        }

        public void UpdateOrder(
            FoodOrder foodOrder)
        {
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }
}