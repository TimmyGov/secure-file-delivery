namespace SecureFileDelivery.Domain.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
