namespace SEVPMS.Application.Features.Food.Validators;

public sealed class FoodOrderValidationException(string message)
    : Exception(message);