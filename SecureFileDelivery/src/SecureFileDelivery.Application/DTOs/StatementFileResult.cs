namespace SecureFileDelivery.Application.DTOs;

public sealed record StatementFileResult(Stream Stream, string FileName, string ContentType);
