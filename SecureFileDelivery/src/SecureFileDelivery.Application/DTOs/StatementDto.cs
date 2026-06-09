namespace SecureFileDelivery.Application.DTOs;

public sealed record StatementDto(
    Guid Id,
    Guid CustomerId,
    string FileName,
    long FileSizeBytes,
    string ContentType,
    DateTime UploadedAt);
