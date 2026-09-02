using SEVPMS.Application.Features.Food.DTOs;
using SEVPMS.Application.Features.Food.Interfaces;
using SEVPMS.Application.Features.Food.Validators;
using SEVPMS.Domain.Entities.Food;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Food.Services;

public sealed class FoodService(
    IFoodRepository foodRepository)
    : IFoodService
{
    public async Task<IReadOnlyList<EventFoodStallDto>>
        GetActiveStallsByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
    {
        var stalls =
            await foodRepository.GetActiveStallsByEventAsync(
                eventId,
                cancellationToken);

        return stalls
            .Select(MapStall)
            .ToList();
    }

    public async Task<IReadOnlyList<EventMenuItemDto>>
        GetMenuByStallAsync(
            Guid eventFoodStallId,
            CancellationToken cancellationToken = default)
    {
        var stall =
            await foodRepository.GetStallByIdAsync(
                eventFoodStallId,
                cancellationToken);

        if (stall is null || !stall.IsActive)
        {
            throw new KeyNotFoundException(
                "Food stall was not found.");
        }

        var eventMenuItems =
            await foodRepository
                .GetAvailableEventMenuItemsByStallAsync(
                    eventFoodStallId,
                    cancellationToken);

        var result = new List<EventMenuItemDto>();

        foreach (var eventMenuItem in eventMenuItems)
        {
            var menuItem =
                await foodRepository.GetMenuItemByIdAsync(
                    eventMenuItem.MenuItemId,
                    cancellationToken);

            if (menuItem is null ||
                !menuItem.IsAvailable ||
                !eventMenuItem.IsAvailable)
            {
                continue;
            }

            var price =
                eventMenuItem.EventPriceOverride
                ?? menuItem.Price;

            result.Add(
                new EventMenuItemDto
                {
                    Id = eventMenuItem.Id,
                    EventFoodStallId =
                        eventMenuItem.EventFoodStallId,
                    MenuItemId = menuItem.Id,
                    Name = menuItem.Name,
                    Description = menuItem.Description,
                    Price = price,
                    Currency = menuItem.Currency,
                    IsAvailable = true,
                    ImageUrl = menuItem.ImageUrl
                });
        }

        return result;
    }

    public async Task<FoodOrderDto> GetOrderByIdAsync(
        Guid customerUserId,
        Guid foodOrderId,
        CancellationToken cancellationToken = default)
    {
        var order =
            await foodRepository.GetOrderByIdAsync(
                foodOrderId,
                cancellationToken);

        if (order is null ||
            order.CustomerUserId != customerUserId)
        {
            throw new KeyNotFoundException(
                "Food order was not found.");
        }

        var items =
            await foodRepository.GetOrderItemsAsync(
                order.Id,
                cancellationToken);

        return MapOrder(
            order,
            items);
    }

    public async Task<IReadOnlyList<FoodOrderDto>>
        GetOrdersByCustomerAsync(
            Guid customerUserId,
            CancellationToken cancellationToken = default)
    {
        var orders =
            await foodRepository.GetOrdersByCustomerAsync(
                customerUserId,
                cancellationToken);

        var result = new List<FoodOrderDto>();

        foreach (var order in orders)
        {
            var items =
                await foodRepository.GetOrderItemsAsync(
                    order.Id,
                    cancellationToken);

            result.Add(
                MapOrder(
                    order,
                    items));
        }

        return result;
    }

    public async Task<IReadOnlyList<FoodOrderStatusHistoryDto>>
        GetOrderStatusHistoryAsync(
            Guid customerUserId,
            Guid foodOrderId,
            CancellationToken cancellationToken = default)
    {
        var order =
            await foodRepository.GetOrderByIdAsync(
                foodOrderId,
                cancellationToken);

        if (order is null ||
            order.CustomerUserId != customerUserId)
        {
            throw new KeyNotFoundException(
                "Food order was not found.");
        }

        var history =
            await foodRepository.GetOrderStatusHistoryAsync(
                foodOrderId,
                cancellationToken);

        return history
            .OrderBy(x => x.ChangedAtUtc)
            .Select(
                x => new FoodOrderStatusHistoryDto
                {
                    Id = x.Id,
                    FoodOrderId = x.FoodOrderId,
                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    ChangedByUserId =
                        x.ChangedByUserId,
                    ChangedAtUtc =
                        x.ChangedAtUtc,
                    Note = x.Note
                })
            .ToList();
    }

    public async Task<FoodOrderDto> CreateOrderAsync(
        Guid customerUserId,
        CreateFoodOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        FoodOrderValidator.ValidateCreate(
            request);

        var stall =
            await foodRepository.GetStallByIdAsync(
                request.EventFoodStallId,
                cancellationToken);

        if (stall is null)
        {
            throw new FoodOrderValidationException(
                "Food stall was not found.");
        }

        if (!stall.IsActive)
        {
            throw new FoodOrderValidationException(
                "Food stall is not active.");
        }

        if (stall.EventId != request.EventId)
        {
            throw new FoodOrderValidationException(
                "Food stall does not belong to the selected event.");
        }

        var now = DateTime.UtcNow;

        if (now < stall.OpensAtUtc ||
            now > stall.ClosesAtUtc)
        {
            throw new FoodOrderValidationException(
                "Food stall is currently closed.");
        }

        var orderId = Guid.NewGuid();

        var orderItems =
            new List<FoodOrderItem>();

        decimal total = 0;

        var groupedItems =
            request.Items
                .GroupBy(x => x.MenuItemId)
                .Select(
                    group => new
                    {
                        MenuItemId = group.Key,
                        Quantity =
                            group.Sum(x => x.Quantity)
                    })
                .ToList();

        foreach (var requestedItem in groupedItems)
        {
            var eventMenuItem =
                await foodRepository.GetEventMenuItemAsync(
                    stall.Id,
                    requestedItem.MenuItemId,
                    cancellationToken);

            if (eventMenuItem is null ||
                !eventMenuItem.IsAvailable)
            {
                throw new FoodOrderValidationException(
                    "A selected menu item is not available.");
            }

            var menuItem =
                await foodRepository.GetMenuItemByIdAsync(
                    requestedItem.MenuItemId,
                    cancellationToken);

            if (menuItem is null ||
                !menuItem.IsAvailable)
            {
                throw new FoodOrderValidationException(
                    "A selected menu item is not available.");
            }

            if (menuItem.VendorId != stall.VendorId)
            {
                throw new FoodOrderValidationException(
                    "A selected menu item does not belong to this food stall.");
            }

            var unitPrice =
                eventMenuItem.EventPriceOverride
                ?? menuItem.Price;

            if (unitPrice < 0)
            {
                throw new FoodOrderValidationException(
                    "Menu item price is invalid.");
            }

            var lineTotal =
                unitPrice *
                requestedItem.Quantity;

            total += lineTotal;

            orderItems.Add(
                new FoodOrderItem
                {
                    Id = Guid.NewGuid(),
                    FoodOrderId = orderId,
                    MenuItemId = menuItem.Id,
                    ItemNameSnapshot =
                        menuItem.Name,
                    UnitPrice = unitPrice,
                    Quantity =
                        requestedItem.Quantity,
                    LineTotal = lineTotal
                });
        }

        var initialStatus =
            FoodOrderStatus.Pending.ToString();

        var order =
            new FoodOrder
            {
                Id = orderId,
                OrderNo = CreateOrderNumber(),
                CustomerUserId = customerUserId,
                EventId = request.EventId,
                EventFoodStallId =
                    request.EventFoodStallId,
                BookingId = request.BookingId,
                Status = initialStatus,
                FulfillmentType =
                    request.FulfillmentType.Trim(),
                SeatLabelSnapshot =
                    string.IsNullOrWhiteSpace(
                        request.SeatLabelSnapshot)
                        ? null
                        : request.SeatLabelSnapshot.Trim(),
                Total = total,
                CreatedAtUtc = now
            };

        var history =
            new FoodOrderStatusHistory
            {
                Id = Guid.NewGuid(),
                FoodOrderId = order.Id,
                OldStatus = string.Empty,
                NewStatus = initialStatus,
                ChangedByUserId =
                    customerUserId,
                ChangedAtUtc = now,
                Note = "Order created."
            };

        await foodRepository.AddOrderAsync(
            order,
            cancellationToken);

        await foodRepository.AddOrderItemsAsync(
            orderItems,
            cancellationToken);

        await foodRepository.AddStatusHistoryAsync(
            history,
            cancellationToken);

        await foodRepository.SaveChangesAsync(
            cancellationToken);

        return MapOrder(
            order,
            orderItems);
    }

    public async Task<FoodOrderDto> UpdateOrderStatusAsync(
        Guid changedByUserId,
        Guid foodOrderId,
        UpdateFoodOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        var order =
            await foodRepository.GetOrderByIdAsync(
                foodOrderId,
                cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException(
                "Food order was not found.");
        }

        FoodOrderValidator.ValidateStatusTransition(
            order.Status,
            request.NewStatus);

        var oldStatus =
            FoodOrderValidator
                .ParseStatus(order.Status)
                .ToString();

        var newStatus =
            FoodOrderValidator
                .ParseStatus(request.NewStatus)
                .ToString();

        order.Status = newStatus;

        foodRepository.UpdateOrder(
            order);

        var history =
            new FoodOrderStatusHistory
            {
                Id = Guid.NewGuid(),
                FoodOrderId = order.Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByUserId =
                    changedByUserId,
                ChangedAtUtc =
                    DateTime.UtcNow,
                Note =
                    string.IsNullOrWhiteSpace(
                        request.Note)
                        ? string.Empty
                        : request.Note.Trim()
            };

        await foodRepository.AddStatusHistoryAsync(
            history,
            cancellationToken);

        await foodRepository.SaveChangesAsync(
            cancellationToken);

        var items =
            await foodRepository.GetOrderItemsAsync(
                order.Id,
                cancellationToken);

        return MapOrder(
            order,
            items);
    }

    private static EventFoodStallDto MapStall(
        EventFoodStall stall)
    {
        return new EventFoodStallDto
        {
            Id = stall.Id,
            EventId = stall.EventId,
            VendorId = stall.VendorId,
            HallLayoutElementId =
                stall.HallLayoutElementId,
            StallName = stall.StallName,
            IsActive = stall.IsActive,
            OpensAtUtc = stall.OpensAtUtc,
            ClosesAtUtc = stall.ClosesAtUtc
        };
    }

    private static FoodOrderDto MapOrder(
        FoodOrder order,
        IReadOnlyCollection<FoodOrderItem> items)
    {
        return new FoodOrderDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            CustomerUserId =
                order.CustomerUserId,
            EventId = order.EventId,
            EventFoodStallId =
                order.EventFoodStallId,
            BookingId = order.BookingId,
            Status = order.Status,
            FulfillmentType =
                order.FulfillmentType,
            SeatLabelSnapshot =
                order.SeatLabelSnapshot,
            Total = order.Total,
            CreatedAtUtc =
                order.CreatedAtUtc,
            Items = items
                .Select(
                    item =>
                        new FoodOrderItemDto
                        {
                            Id = item.Id,
                            MenuItemId =
                                item.MenuItemId,
                            ItemNameSnapshot =
                                item.ItemNameSnapshot,
                            UnitPrice =
                                item.UnitPrice,
                            Quantity =
                                item.Quantity,
                            LineTotal =
                                item.LineTotal
                        })
                .ToList()
        };
    }

    private static string CreateOrderNumber()
    {
        return
            $"FO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }
}