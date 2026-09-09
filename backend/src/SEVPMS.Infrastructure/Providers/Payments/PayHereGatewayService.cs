using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Application.Features.Payments.Interfaces;
using SEVPMS.Domain.Entities.Payments;

namespace SEVPMS.Infrastructure.Providers.Payments;

public sealed class PayHereGatewayService(IConfiguration configuration) : IPayHereGatewayService
{
    public PayHereCheckoutResponse CreateCheckout(Payment payment, PayHereCustomerDetails customer)
    {
        var merchantId = Required("Payments:PayHere:MerchantId");
        var merchantSecret = Required("Payments:PayHere:MerchantSecret");
        var returnUrl = Required("Payments:PayHere:ReturnUrl");
        var cancelUrl = Required("Payments:PayHere:CancelUrl");
        var notifyUrl = Required("Payments:PayHere:NotifyUrl");
        var sandboxValue = configuration["Payments:PayHere:Sandbox"];
        var sandbox = !bool.TryParse(sandboxValue, out var parsedSandbox) || parsedSandbox;

        var amount = payment.Amount.ToString("0.00", CultureInfo.InvariantCulture);
        var currency = payment.Currency.Trim().ToUpperInvariant();
        var orderId = payment.CheckoutReference;
        var checkoutHash = Md5(merchantId + orderId + amount + currency + Md5(merchantSecret));

        return new PayHereCheckoutResponse
        {
            PaymentId = payment.Id,
            CheckoutUrl = sandbox ? "https://sandbox.payhere.lk/pay/checkout" : "https://www.payhere.lk/pay/checkout",
            MerchantId = merchantId,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,
            NotifyUrl = notifyUrl,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = configuration["Payments:PayHere:DefaultAddress"] ?? "Not provided",
            City = configuration["Payments:PayHere:DefaultCity"] ?? "Colombo",
            Country = configuration["Payments:PayHere:DefaultCountry"] ?? "Sri Lanka",
            OrderId = orderId,
            Items = "Nvent event booking",
            Amount = amount,
            Currency = currency,
            Hash = checkoutHash
        };
    }

    public bool VerifyNotification(PayHereNotifyRequest request)
    {
        var merchantId = Required("Payments:PayHere:MerchantId");
        var merchantSecret = Required("Payments:PayHere:MerchantSecret");
        if (!string.Equals(request.MerchantId, merchantId, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(request.Md5Sig)) return false;
        var local = Md5(request.MerchantId + request.OrderId + request.PayHereAmount + request.PayHereCurrency + request.StatusCode + Md5(merchantSecret));
        var remote = request.Md5Sig.Trim().ToUpperInvariant();
        if (local.Length != remote.Length) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(local), Encoding.ASCII.GetBytes(remote));
    }

    public string HashNotificationPayload(PayHereNotifyRequest request)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", request.MerchantId, request.OrderId, request.PaymentId, request.PayHereAmount, request.PayHereCurrency, request.StatusCode))));

    private string Required(string key)
        => configuration[key] is { } value && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Required PayHere configuration '{key}' is missing.");

    private static string Md5(string value)
        => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(value)));
}
