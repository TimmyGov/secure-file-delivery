using SecureFileDelivery.Domain.Enums;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public StatementId StatementId { get; private set; }
    public TokenId? TokenId { get; private set; }
    public AuditAction Action { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string PerformedBy { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;

    private AuditLog()
    {
    }

    public AuditLog(Guid id, StatementId statementId, TokenId? tokenId, AuditAction action, DateTime performedAt, string performedBy, string ipAddress, string details)
    {
        Id = id;
        StatementId = statementId;
        TokenId = tokenId;
        Action = action;
        PerformedAt = performedAt;
        PerformedBy = performedBy;
        IpAddress = ipAddress;
        Details = details;
    }
}
