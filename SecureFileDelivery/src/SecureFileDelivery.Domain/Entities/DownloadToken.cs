using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Entities;

public class DownloadToken
{
    public Guid Id { get; private set; }
    public StatementId StatementId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public bool IsMultiUse { get; private set; }

    private DownloadToken()
    {
    }

    public DownloadToken(Guid id, StatementId statementId, string tokenHash, DateTime expiresAt, DateTime createdAt, bool isMultiUse)
    {
        Id = id;
        StatementId = statementId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        IsMultiUse = isMultiUse;
    }

    public bool IsExpired() => ExpiresAt < DateTime.UtcNow;

    public bool IsRedeemable() => !IsExpired() && !IsRevoked && (IsMultiUse || UsedAt is null);

    public void MarkAsUsed() => UsedAt = DateTime.UtcNow;

    public void Revoke() => IsRevoked = true;
}
