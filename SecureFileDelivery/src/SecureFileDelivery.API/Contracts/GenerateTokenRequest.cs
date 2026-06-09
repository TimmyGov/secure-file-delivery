namespace SecureFileDelivery.API.Contracts;

public sealed record GenerateTokenRequest(int TtlMinutes = 60, bool IsMultiUse = false);
