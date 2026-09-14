using SEVPMS.Application.Features.Food.DTOs;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Food.Validators;

public static class FoodOrderValidator
{
    public static void ValidateCreate(
        CreateFoodOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EventId == Guid.Empty)
        {
            throw new FoodOrderValidationException(
                "Event is required.");
        }

        if (request.EventFoodStallId == Guid.Empty)
        {
            throw new FoodOrderValidationException(
                "Food stall is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FulfillmentType))
        {
            throw new FoodOrderValidationException(
                "Fulfillment type is required.");
        }

        if (request.Items.Count == 0)
        {
            throw new FoodOrderValidationException(
                "At least one food item is required.");
        }

        foreach (var item in request.Items)
        {
            if (item.MenuItemId == Guid.Empty)
            {
                throw new FoodOrderValidationException(
                    "Menu item is required.");
            }

            if (item.Quantity <= 0)
            {
                throw new FoodOrderValidationException(
                    "Item quantity must be greater than zero.");
            }
        }
    }

    public static FoodOrderStatus ParseStatus(
        string status)
    {
        if (!Enum.TryParse<FoodOrderStatus>(
                status,
                true,
                out var parsedStatus))
        {
            throw new FoodOrderValidationException(
                "Invalid food order status.");
        }

        return parsedStatus;
    }

    public static void ValidateStatusTransition(
        string currentStatus,
        string newStatus)
    {
        var current = ParseStatus(currentStatus);
        var next = ParseStatus(newStatus);

        if (current == next)
        {
            throw new FoodOrderValidationException(
                "Food order is already in the requested status.");
        }

        var allowed = current switch
        {
            FoodOrderStatus.Placed =>
                next is FoodOrderStatus.Accepted
                    or FoodOrderStatus.Rejected
                    or FoodOrderStatus.Cancelled,

            FoodOrderStatus.Accepted =>
                next is FoodOrderStatus.Preparing
                    or FoodOrderStatus.Cancelled,

            FoodOrderStatus.Preparing =>
                next is FoodOrderStatus.Ready,

            FoodOrderStatus.Ready =>
                next is FoodOrderStatus.Completed,

            _ => false
        };

        if (!allowed)
        {
            throw new FoodOrderValidationException(
                $"Food order cannot change from {current} to {next}.");
        }
    }
}