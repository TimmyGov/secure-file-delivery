namespace SecureFileDelivery.Infrastructure.Services;

public sealed class TokenCleanupSettings
{
    public int IntervalMinutes { get; set; } = 60;
}
