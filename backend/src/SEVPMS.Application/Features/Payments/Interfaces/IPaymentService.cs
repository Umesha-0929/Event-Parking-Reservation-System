using SEVPMS.Application.Features.Payments.DTOs;

namespace SEVPMS.Application.Features.Payments.Interfaces;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentResponse>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> StartAsync(Guid customerUserId, StartPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse> CompleteMockAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> FailMockAsync(Guid paymentId, CancellationToken cancellationToken = default);
}
