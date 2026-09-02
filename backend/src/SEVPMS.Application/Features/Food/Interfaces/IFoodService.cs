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