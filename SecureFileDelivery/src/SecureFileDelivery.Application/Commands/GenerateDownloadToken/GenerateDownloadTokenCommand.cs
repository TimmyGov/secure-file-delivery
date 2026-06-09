using MediatR;
using SecureFileDelivery.Application.DTOs;

namespace SecureFileDelivery.Application.Commands.GenerateDownloadToken;

public sealed record GenerateDownloadTokenCommand(
    Guid StatementId,
    string RequestedBy,
    int TtlMinutes = 60,
    bool IsMultiUse = false) : IRequest<DownloadTokenDto>;
