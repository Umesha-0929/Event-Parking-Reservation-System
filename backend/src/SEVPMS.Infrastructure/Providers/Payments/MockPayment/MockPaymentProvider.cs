using SEVPMS.Application.Interfaces.Providers;

namespace SEVPMS.Infrastructure.Providers.Payments.MockPayment;

public sealed class MockPaymentProvider : IPaymentProvider
{
    public Task<string> CreateCheckoutAsync(
        Guid bookingId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var reference = $"MOCK-{bookingId:N}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        return Task.FromResult(reference);
    }
}
