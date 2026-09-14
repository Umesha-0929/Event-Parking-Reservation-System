using Microsoft.AspNetCore.Authorization;
using SEVPMS.Realtime.Hubs;
using Xunit;

namespace SEVPMS.IntegrationTests.Realtime;

public sealed class RealtimeAuthorizationContractTests
{
    [Fact]
    public void NotificationHub_requires_authorization()
        => Assert.NotNull(
            Attribute.GetCustomAttribute(
                typeof(NotificationHub),
                typeof(AuthorizeAttribute)));

    [Fact]
    public void EventHub_requires_authorization()
        => Assert.NotNull(
            Attribute.GetCustomAttribute(
                typeof(EventHub),
                typeof(AuthorizeAttribute)));
}
