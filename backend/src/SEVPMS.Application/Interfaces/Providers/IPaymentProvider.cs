namespace SEVPMS.Application.Interfaces.Providers;

public interface IPaymentProvider
{
    Task<string> CreateCheckoutAsync(Guid bookingId, decimal amount, CancellationToken cancellationToken = default);
}
