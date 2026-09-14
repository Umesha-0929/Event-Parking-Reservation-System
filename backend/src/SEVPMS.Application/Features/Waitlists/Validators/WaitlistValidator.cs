namespace SEVPMS.Application.Features.Waitlists.Validators;

public static class WaitlistValidator
{
    public static void ValidateEventId(Guid eventId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event is required.");
    }

    public static void ValidateCustomerId(Guid customerUserId)
    {
        if (customerUserId == Guid.Empty)
            throw new ArgumentException("Customer is required.");
    }

    public static void ValidateAvailableCount(int availableCount)
    {
        if (availableCount <= 0)
            throw new ArgumentException(
                "Available count must be greater than zero.");
    }
}