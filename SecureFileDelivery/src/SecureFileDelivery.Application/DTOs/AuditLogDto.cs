namespace SecureFileDelivery.Application.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    Guid StatementId,
    Guid? TokenId,
    string Action,
    DateTime PerformedAt,
    string PerformedBy,
    string IpAddress,
    string Details);
