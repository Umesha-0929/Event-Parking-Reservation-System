using Xunit;
using SEVPMS.Domain.Common;

namespace SEVPMS.UnitTests;

public sealed class SmokeTests
{
    private sealed class TestEntity : BaseEntity;

    [Fact]
    public void BaseEntity_CreatesANonEmptyId()
    {
        var entity = new TestEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
    }
}
