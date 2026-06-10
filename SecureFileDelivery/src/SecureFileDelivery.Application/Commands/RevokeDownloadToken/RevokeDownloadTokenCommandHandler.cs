using MediatR;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Enums;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Commands.RevokeDownloadToken;

public sealed class RevokeDownloadTokenCommandHandler : IRequestHandler<RevokeDownloadTokenCommand, bool>
{
    private readonly IDownloadTokenRepository _downloadTokenRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RevokeDownloadTokenCommandHandler(
        IDownloadTokenRepository downloadTokenRepository,
        IAuditLogRepository auditLogRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _downloadTokenRepository = downloadTokenRepository;
        _auditLogRepository = auditLogRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<bool> Handle(RevokeDownloadTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _downloadTokenRepository.GetByIdAsync(new TokenId(request.TokenId));
        if (token is null)
        {
            throw new TokenNotFoundException();
        }

        token.Revoke();
        await _downloadTokenRepository.UpdateAsync(token);
        await _auditLogRepository.AddAsync(new AuditLog(
            Guid.NewGuid(),
            token.StatementId,
            new TokenId(token.Id),
            AuditAction.TokenRevoked,
            _dateTimeProvider.UtcNow,
            request.RevokedBy,
            "n/a",
            "Download token revoked."));

        return true;
    }
}
