using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Application.Features.Payments.Interfaces;

public interface IPayHereGatewayService
{
    PayHereCheckoutResponse CreateCheckout(Payment payment, PayHereCustomerDetails customer);
    bool VerifyNotification(PayHereNotifyRequest request);
    string HashNotificationPayload(PayHereNotifyRequest request);
}
