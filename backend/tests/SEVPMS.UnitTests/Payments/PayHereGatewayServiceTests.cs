using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SEVPMS.Application.Features.Payments.DTOs;
using SEVPMS.Infrastructure.Providers.Payments;
using Xunit;

namespace SEVPMS.UnitTests.Payments;

public sealed class PayHereGatewayServiceTests
{
    [Fact]
    public void VerifyNotification_accepts_valid_checksum()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Payments:PayHere:MerchantId"] = "121XXXX",
                    ["Payments:PayHere:MerchantSecret"] = "sandbox-secret",
                    ["Payments:PayHere:Sandbox"] = "true"
                })
            .Build();

        var request = new PayHereNotifyRequest
        {
            MerchantId = "121XXXX",
            OrderId = "ORDER-1",
            PaymentId = "320000001",
            PayHereAmount = "1500.00",
            PayHereCurrency = "LKR",
            StatusCode = "2"
        };

        static string Md5(string value)
            => Convert.ToHexString(
                MD5.HashData(Encoding.UTF8.GetBytes(value)));

        request.Md5Sig = Md5(
            request.MerchantId +
            request.OrderId +
            request.PayHereAmount +
            request.PayHereCurrency +
            request.StatusCode +
            Md5("sandbox-secret"));

        var verifier = new PayHereGatewayService(configuration);

        Assert.True(verifier.VerifyNotification(request));
    }

    [Fact]
    public void VerifyNotification_rejects_tampered_checksum()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Payments:PayHere:MerchantId"] = "121XXXX",
                    ["Payments:PayHere:MerchantSecret"] = "sandbox-secret"
                })
            .Build();

        var request = new PayHereNotifyRequest
        {
            MerchantId = "121XXXX",
            OrderId = "ORDER-1",
            PaymentId = "320000001",
            PayHereAmount = "1500.00",
            PayHereCurrency = "LKR",
            StatusCode = "2",
            Md5Sig = new string('0', 32)
        };

        var verifier = new PayHereGatewayService(configuration);

        Assert.False(verifier.VerifyNotification(request));
    }
}
