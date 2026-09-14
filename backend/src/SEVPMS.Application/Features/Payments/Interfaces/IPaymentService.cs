using SEVPMS.Application.Features.Payments.DTOs;

namespace SEVPMS.Application.Features.Payments.Interfaces;

public interface IPaymentService
{
    Task<IReadOnlyList<PaymentResponse>> GetMineAsync(Guid customerUserId, CancellationToken cancellationToken = default);
    async Task<IReadOnlyList<PaymentResponse>> GetMinePageAsync(
        Guid customerUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var all = await GetMineAsync(customerUserId, cancellationToken);
        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        return all.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToArray();
    }
    Task<PaymentResponse> StartAsync(Guid customerUserId, StartPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResponse> CompleteMockAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> FailMockAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> ProcessSandboxCallbackAsync(SandboxPaymentCallbackRequest request, CancellationToken cancellationToken = default);
    Task<PayHereCheckoutResponse> GetPayHereCheckoutAsync(Guid customerUserId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> ProcessPayHereNotificationAsync(PayHereNotifyRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentTransactionResponse>> GetTransactionsAsync(Guid customerUserId, Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> SubmitManualProofAsync(Guid customerUserId, Guid paymentId, ManualPaymentProofRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ManualPaymentReviewResponse>> GetManualReviewsAsync(Guid reviewerUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<PaymentResponse> ApproveManualAsync(Guid reviewerUserId, bool isAdmin, Guid paymentId, CancellationToken cancellationToken = default);
    Task<PaymentResponse> RejectManualAsync(Guid reviewerUserId, bool isAdmin, Guid paymentId, CancellationToken cancellationToken = default);
}
