using FluentAssertions;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Tests;

public sealed class DownloadTokenTests
{
    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExpirationIsInThePast()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(-2), false);

        token.IsExpired().Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenExpirationIsInTheFuture()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, false);

        token.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void IsRedeemable_ShouldReturnFalse_WhenExpired()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow.AddMinutes(-10), false);

        token.IsRedeemable().Should().BeFalse();
    }

    [Fact]
    public void IsRedeemable_ShouldReturnFalse_WhenRevoked()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, false);
        token.Revoke();

        token.IsRedeemable().Should().BeFalse();
    }

    [Fact]
    public void IsRedeemable_ShouldReturnFalse_WhenUsedAndSingleUse()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, false);
        token.MarkAsUsed();

        token.IsRedeemable().Should().BeFalse();
    }

    [Fact]
    public void IsRedeemable_ShouldReturnTrue_WhenValidAndUnused()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, false);

        token.IsRedeemable().Should().BeTrue();
    }

    [Fact]
    public void IsRedeemable_ShouldReturnTrue_WhenMultiUseAndUsed()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, true);
        token.MarkAsUsed();

        token.IsRedeemable().Should().BeTrue();
    }

    [Fact]
    public void MarkAsUsed_ShouldSetUsedAt()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, false);

        token.MarkAsUsed();

        token.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public void Revoke_ShouldSetIsRevokedTrue()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow, false);

        token.Revoke();

        token.IsRevoked.Should().BeTrue();
    }
}
