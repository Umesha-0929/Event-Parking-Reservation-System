using SEVPMS.Application.Features.Payments.DTOs;

namespace SEVPMS.Application.Features.Payments.Interfaces;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentResponse>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> StartAsync(Guid customerUserId, StartPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse> CompleteMockAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> FailMockAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> ProcessSandboxCallbackAsync(SandboxPaymentCallbackRequest request, CancellationToken cancellationToken = default);
    Task<PayHereCheckoutResponse> GetPayHereCheckoutAsync(Guid customerUserId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> ProcessPayHereNotificationAsync(PayHereNotifyRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentTransactionResponse>> GetTransactionsAsync(Guid customerUserId, Guid paymentId, CancellationToken cancellationToken = default);
}
