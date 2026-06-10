using MediatR;
using SecureFileDelivery.Application.DTOs;

namespace SecureFileDelivery.Application.Commands.UploadStatement;

public sealed record UploadStatementCommand(
    Guid CustomerId,
    string FileName,
    Stream FileStream,
    string ContentType,
    long FileSizeBytes) : IRequest<StatementDto>;
