using SEVPMS.Domain.Entities.Food;

namespace SEVPMS.Application.Features.Food.Interfaces;

public interface IFoodRepository
{
    Task<IReadOnlyList<EventFoodStall>> GetActiveStallsByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<EventFoodStall?> GetStallByIdAsync(
        Guid eventFoodStallId,
        CancellationToken cancellationToken = default);

    Task<FoodVendor?> GetVendorByIdAsync(
        Guid vendorId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventMenuItem>> GetAvailableEventMenuItemsByStallAsync(
        Guid eventFoodStallId,
        CancellationToken cancellationToken = default);

    Task<MenuItem?> GetMenuItemByIdAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    Task<EventMenuItem?> GetEventMenuItemAsync(
        Guid eventFoodStallId,
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    Task<FoodOrder?> GetOrderByIdAsync(
        Guid foodOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FoodOrder>> GetOrdersByCustomerAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FoodOrderItem>> GetOrderItemsAsync(
        Guid foodOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FoodOrderStatusHistory>> GetOrderStatusHistoryAsync(
        Guid foodOrderId,
        CancellationToken cancellationToken = default);

    Task AddOrderAsync(
        FoodOrder foodOrder,
        CancellationToken cancellationToken = default);

    Task AddOrderItemsAsync(
        IReadOnlyCollection<FoodOrderItem> items,
        CancellationToken cancellationToken = default);

    Task AddStatusHistoryAsync(
        FoodOrderStatusHistory history,
        CancellationToken cancellationToken = default);

    void UpdateOrder(
        FoodOrder foodOrder);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}