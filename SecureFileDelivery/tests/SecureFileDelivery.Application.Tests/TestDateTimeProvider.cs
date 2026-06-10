using SecureFileDelivery.Domain.Interfaces;

namespace SecureFileDelivery.Application.Tests;

internal sealed class TestDateTimeProvider : IDateTimeProvider
{
    public TestDateTimeProvider(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; }
}
