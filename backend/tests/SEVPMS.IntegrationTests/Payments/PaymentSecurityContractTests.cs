using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using SEVPMS.Api.Controllers;
using Xunit;

namespace SEVPMS.IntegrationTests.Payments;

public sealed class PaymentSecurityContractTests
{
    [Fact]
    public void PayHere_notify_is_server_callback_endpoint()
    {
        var method = typeof(PaymentsController).GetMethod("PayHereNotify");

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void Transaction_history_endpoint_exists()
    {
        Assert.NotNull(typeof(PaymentsController).GetMethod("Transactions"));
    }

    [Fact]
    public void PayHere_checkout_requires_customer_authorization()
    {
        var method = typeof(PaymentsController).GetMethod("PayHereCheckout");

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<AuthorizeAttribute>());
    }
}
