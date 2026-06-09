using SecureFileDelivery.Application.DTOs;
using SecureFileDelivery.Domain.Entities;

namespace SecureFileDelivery.Application.Common.Mappings;

public static class DomainMappings
{
    public static StatementDto ToDto(this Statement statement) => new(
        statement.Id,
        statement.CustomerId.Value,
        statement.FileName,
        statement.FileSizeBytes,
        statement.ContentType,
        statement.UploadedAt);

    public static DownloadTokenDto ToDto(this DownloadToken token, string? rawToken = null) => new(
        token.Id,
        token.StatementId.Value,
        rawToken,
        token.ExpiresAt,
        token.IsMultiUse);

    public static AuditLogDto ToDto(this AuditLog log) => new(
        log.Id,
        log.StatementId.Value,
        log.TokenId?.Value,
        log.Action.ToString(),
        log.PerformedAt,
        log.PerformedBy,
        log.IpAddress,
        log.Details);
}
