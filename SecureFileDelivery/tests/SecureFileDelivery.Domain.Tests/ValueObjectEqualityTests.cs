using FluentAssertions;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Tests;

public sealed class ValueObjectEqualityTests
{
    [Fact]
    public void CustomerId_ShouldSupportValueEquality()
    {
        var value = Guid.NewGuid();
        new CustomerId(value).Should().Be(new CustomerId(value));
    }

    [Fact]
    public void StatementId_ShouldSupportValueEquality()
    {
        var value = Guid.NewGuid();
        new StatementId(value).Should().Be(new StatementId(value));
    }

    [Fact]
    public void TokenId_ShouldSupportValueEquality()
    {
        var value = Guid.NewGuid();
        new TokenId(value).Should().Be(new TokenId(value));
    }
}
