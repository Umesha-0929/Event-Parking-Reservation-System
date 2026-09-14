using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using SEVPMS.Api.Controllers;
using Xunit;

namespace SEVPMS.IntegrationTests.Auth;

public sealed class AuthSecurityContractTests
{
    [Fact]
    public void Logout_requires_authorization()
    {
        var method = typeof(AuthController).GetMethod("Logout");

        Assert.NotNull(method);
        Assert.NotNull(method!.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void Password_reset_endpoints_are_available_without_login()
    {
        var request = typeof(AuthController).GetMethod("RequestPasswordReset");
        var confirm = typeof(AuthController).GetMethod("ConfirmPasswordReset");

        Assert.NotNull(request);
        Assert.NotNull(confirm);
        Assert.NotNull(request!.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.NotNull(confirm!.GetCustomAttribute<AllowAnonymousAttribute>());
    }
}
