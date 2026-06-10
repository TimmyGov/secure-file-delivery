namespace SecureFileDelivery.Domain.Enums;

public enum AuditAction
{
    Uploaded,
    TokenGenerated,
    TokenRedeemed,
    TokenRevoked,
    TokenExpired
}
