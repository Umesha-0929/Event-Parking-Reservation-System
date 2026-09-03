using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Payments.Interfaces;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Infrastructure.Providers.Payments;

public sealed class PayHereGatewayService(
    IConfiguration configuration)
    : IPayHereGatewayService
{
    public PayHereCheckoutResponse CreateCheckout(Payment payment)
    {
        var merchantId = Required("Payments:PayHere:MerchantId");
        var merchantSecret = Required("Payments:PayHere:MerchantSecret");
        var sandboxValue = configuration["Payments:PayHere:Sandbox"];
        var sandbox = !bool.TryParse(sandboxValue, out var parsedSandbox) || parsedSandbox;

        var amount = payment.Amount.ToString("0.00", CultureInfo.InvariantCulture);
        var currency = payment.Currency.Trim().ToUpperInvariant();
        var orderId = payment.CheckoutReference;

        var secretHash = Md5(merchantSecret);
        var checkoutHash = Md5(
            merchantId +
            orderId +
            amount +
            currency +
            secretHash);

        return new PayHereCheckoutResponse
        {
            PaymentId = payment.Id,
            CheckoutUrl = sandbox
                ? "https://sandbox.payhere.lk/pay/checkout"
                : "https://www.payhere.lk/pay/checkout",
            MerchantId = merchantId,
            OrderId = orderId,
            Amount = amount,
            Currency = currency,
            Hash = checkoutHash
        };
    }

    public bool VerifyNotification(PayHereNotifyRequest request)
    {
        var merchantId = Required("Payments:PayHere:MerchantId");
        var merchantSecret = Required("Payments:PayHere:MerchantSecret");

        if (!string.Equals(
                request.MerchantId,
                merchantId,
                StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(request.Md5Sig))
        {
            return false;
        }

        var local = Md5(
            request.MerchantId +
            request.OrderId +
            request.PayHereAmount +
            request.PayHereCurrency +
            request.StatusCode +
            Md5(merchantSecret));

        var remote = request.Md5Sig.Trim().ToUpperInvariant();

        if (local.Length != remote.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(local),
            Encoding.ASCII.GetBytes(remote));
    }

    public string HashNotificationPayload(PayHereNotifyRequest request)
        => Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    string.Join(
                        "|",
                        request.MerchantId,
                        request.OrderId,
                        request.PaymentId,
                        request.PayHereAmount,
                        request.PayHereCurrency,
                        request.StatusCode))));

    private string Required(string key)
        => configuration[key]
           ?? throw new InvalidOperationException(
               $"Required PayHere configuration '{key}' is missing.");

    private static string Md5(string value)
        => Convert.ToHexString(
            MD5.HashData(Encoding.UTF8.GetBytes(value)));
}
