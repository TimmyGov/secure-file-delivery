using SecureFileDelivery.Domain.Interfaces;

namespace SecureFileDelivery.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
