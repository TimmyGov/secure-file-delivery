namespace SecureFileDelivery.Application.DTOs;

public sealed record DownloadTokenDto(
    Guid Id,
    Guid StatementId,
    string? RawToken,
    DateTime ExpiresAt,
    bool IsMultiUse);
