using Microsoft.EntityFrameworkCore;
using SEVPMS.Application.Features.Food.Interfaces;
using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Infrastructure.Persistence.Repositories;

public sealed class FoodRepository(
    SEVPMSDbContext dbContext)
    : IFoodRepository
{
    public async Task<IReadOnlyList<EventFoodStall>>
        GetActiveStallsByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<EventFoodStall>()
            .Where(stall =>
                stall.EventId == eventId &&
                stall.IsActive)
            .OrderBy(stall => stall.StallName)
            .ToListAsync(cancellationToken);
    }

    public async Task<EventFoodStall?> GetStallByIdAsync(
        Guid eventFoodStallId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<EventFoodStall>()
            .SingleOrDefaultAsync(
                stall =>
                    stall.Id == eventFoodStallId,
                cancellationToken);
    }

    public async Task<FoodVendor?> GetVendorByIdAsync(
        Guid vendorId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<FoodVendor>()
            .SingleOrDefaultAsync(
                vendor =>
                    vendor.Id == vendorId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<EventMenuItem>>
        GetAvailableEventMenuItemsByStallAsync(
            Guid eventFoodStallId,
            CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<EventMenuItem>()
            .Where(item =>
                item.EventFoodStallId == eventFoodStallId &&
                item.IsAvailable)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItem?> GetMenuItemByIdAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<MenuItem>()
            .SingleOrDefaultAsync(
                item =>
                    item.Id == menuItemId,
                cancellationToken);
    }

    public async Task<EventMenuItem?> GetEventMenuItemAsync(
        Guid eventFoodStallId,
        Guid menuItemId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<EventMenuItem>()
            .SingleOrDefaultAsync(
                item =>
                    item.EventFoodStallId == eventFoodStallId &&
                    item.MenuItemId == menuItemId,
                cancellationToken);
    }

    public async Task<FoodOrder?> GetOrderByIdAsync(
        Guid foodOrderId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<FoodOrder>()
            .SingleOrDefaultAsync(
                order =>
                    order.Id == foodOrderId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<FoodOrder>>
        GetOrdersByCustomerAsync(
            Guid customerUserId,
            CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<FoodOrder>()
            .Where(order =>
                order.CustomerUserId == customerUserId)
            .OrderByDescending(order =>
                order.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FoodOrderItem>>
        GetOrderItemsAsync(
            Guid foodOrderId,
            CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<FoodOrderItem>()
            .Where(item =>
                item.FoodOrderId == foodOrderId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FoodOrderStatusHistory>>
        GetOrderStatusHistoryAsync(
            Guid foodOrderId,
            CancellationToken cancellationToken = default)
    {
        return await dbContext
            .Set<FoodOrderStatusHistory>()
            .Where(history =>
                history.FoodOrderId == foodOrderId)
            .OrderBy(history =>
                history.ChangedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddOrderAsync(
        FoodOrder foodOrder,
        CancellationToken cancellationToken = default)
    {
        await dbContext
            .Set<FoodOrder>()
            .AddAsync(
                foodOrder,
                cancellationToken);
    }

    public async Task AddOrderItemsAsync(
        IReadOnlyCollection<FoodOrderItem> items,
        CancellationToken cancellationToken = default)
    {
        await dbContext
            .Set<FoodOrderItem>()
            .AddRangeAsync(
                items,
                cancellationToken);
    }

    public async Task AddStatusHistoryAsync(
        FoodOrderStatusHistory history,
        CancellationToken cancellationToken = default)
    {
        await dbContext
            .Set<FoodOrderStatusHistory>()
            .AddAsync(
                history,
                cancellationToken);
    }

    public void UpdateOrder(
        FoodOrder foodOrder)
    {
        dbContext
            .Set<FoodOrder>()
            .Update(foodOrder);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}