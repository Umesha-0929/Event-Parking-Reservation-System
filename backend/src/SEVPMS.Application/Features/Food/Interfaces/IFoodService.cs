using SEVPMS.Application.Features.Food.DTOs;

namespace SEVPMS.Application.Features.Food.Interfaces;

public interface IFoodService
{
    Task<IReadOnlyList<EventFoodStallDto>> GetActiveStallsByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventMenuItemDto>> GetMenuByStallAsync(
        Guid eventFoodStallId,
        CancellationToken cancellationToken = default);

    Task<FoodOrderDto> GetOrderByIdAsync(
        Guid customerUserId,
        Guid foodOrderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FoodOrderDto>> GetOrdersByCustomerAsync(
        Guid customerUserId,
        CancellationToken cancellationToken = default);

    async Task<IReadOnlyList<FoodOrderDto>> GetOrdersByCustomerPageAsync(
        Guid customerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetOrdersByCustomerAsync(customerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }

    Task<IReadOnlyList<FoodOrderStatusHistoryDto>> GetOrderStatusHistoryAsync(
        Guid customerUserId,
        Guid foodOrderId,
        CancellationToken cancellationToken = default);

    Task<FoodOrderDto> CreateOrderAsync(
        Guid customerUserId,
        CreateFoodOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<FoodOrderDto> UpdateOrderStatusAsync(
        Guid changedByUserId,
        Guid foodOrderId,
        UpdateFoodOrderStatusRequest request,
        CancellationToken cancellationToken = default);
}