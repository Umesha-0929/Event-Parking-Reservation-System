using SEVPMS.Application.Features.Food.DTOs;
using SEVPMS.Application.Features.Food.Validators;
using Xunit;

namespace SEVPMS.UnitTests.Food;

public sealed class FoodOrderValidatorTests
{
    [Fact]
    public void ValidateCreate_WithValidRequest_DoesNotThrow()
    {
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
                    Quantity = 2
                }
            ]
        };

        var exception = Record.Exception(
            () => FoodOrderValidator.ValidateCreate(request));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateCreate_WithEmptyEventId_ThrowsValidationException()
    {
        var request = CreateValidRequest();
        request.EventId = Guid.Empty;

        var exception =
            Assert.Throws<FoodOrderValidationException>(
                () => FoodOrderValidator.ValidateCreate(request));

        Assert.Equal(
            "Event is required.",
            exception.Message);
    }

    [Fact]
    public void ValidateCreate_WithEmptyFoodStallId_ThrowsValidationException()
    {
        var request = CreateValidRequest();
        request.EventFoodStallId = Guid.Empty;

        var exception =
            Assert.Throws<FoodOrderValidationException>(
                () => FoodOrderValidator.ValidateCreate(request));

        Assert.Equal(
            "Food stall is required.",
            exception.Message);
    }

    [Fact]
    public void ValidateCreate_WithNoItems_ThrowsValidationException()
    {
        var request = CreateValidRequest();
        request.Items = [];

        var exception =
            Assert.Throws<FoodOrderValidationException>(
                () => FoodOrderValidator.ValidateCreate(request));

        Assert.Equal(
            "At least one food item is required.",
            exception.Message);
    }

    [Fact]
    public void ValidateCreate_WithZeroQuantity_ThrowsValidationException()
    {
        var request = CreateValidRequest();
        request.Items[0].Quantity = 0;

        var exception =
            Assert.Throws<FoodOrderValidationException>(
                () => FoodOrderValidator.ValidateCreate(request));

        Assert.Equal(
            "Item quantity must be greater than zero.",
            exception.Message);
    }

    [Theory]
    [InlineData("Placed", "Accepted")]
    [InlineData("Placed", "Rejected")]
    [InlineData("Placed", "Cancelled")]
    [InlineData("Accepted", "Preparing")]
    [InlineData("Accepted", "Cancelled")]
    [InlineData("Preparing", "Ready")]
    [InlineData("Ready", "Completed")]
    public void ValidateStatusTransition_WithAllowedTransition_DoesNotThrow(
        string currentStatus,
        string newStatus)
    {
        var exception = Record.Exception(
            () => FoodOrderValidator.ValidateStatusTransition(
                currentStatus,
                newStatus));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Placed", "Ready")]
    [InlineData("Preparing", "Completed")]
    [InlineData("Completed", "Placed")]
    [InlineData("Cancelled", "Accepted")]
    [InlineData("Rejected", "Accepted")]
    public void ValidateStatusTransition_WithInvalidTransition_ThrowsValidationException(
        string currentStatus,
        string newStatus)
    {
        Assert.Throws<FoodOrderValidationException>(
            () => FoodOrderValidator.ValidateStatusTransition(
                currentStatus,
                newStatus));
    }

    [Fact]
    public void ParseStatus_WithInvalidStatus_ThrowsValidationException()
    {
        Assert.Throws<FoodOrderValidationException>(
            () => FoodOrderValidator.ParseStatus("Unknown"));
    }

    private static CreateFoodOrderRequest CreateValidRequest()
    {
        return new CreateFoodOrderRequest
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
    }
}