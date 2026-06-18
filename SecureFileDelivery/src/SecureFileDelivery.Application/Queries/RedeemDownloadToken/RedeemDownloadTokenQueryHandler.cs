using MediatR;
using SecureFileDelivery.Application.DTOs;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Enums;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Queries.RedeemDownloadToken;

public sealed class RedeemDownloadTokenQueryHandler : IRequestHandler<RedeemDownloadTokenQuery, StatementFileResult>
{
    private readonly IDownloadTokenRepository _downloadTokenRepository;
    private readonly IStatementRepository _statementRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ITokenHasher _tokenHasher;
    private readonly IFileStorage _fileStorage;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RedeemDownloadTokenQueryHandler(
        IDownloadTokenRepository downloadTokenRepository,
        IStatementRepository statementRepository,
        IAuditLogRepository auditLogRepository,
        ITokenHasher tokenHasher,
        IFileStorage fileStorage,
        IDateTimeProvider dateTimeProvider)
    {
        _downloadTokenRepository = downloadTokenRepository;
        _statementRepository = statementRepository;
        _auditLogRepository = auditLogRepository;
        _tokenHasher = tokenHasher;
        _fileStorage = fileStorage;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<StatementFileResult> Handle(RedeemDownloadTokenQuery request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenHasher.Hash(request.RawToken);
        var token = await _downloadTokenRepository.GetByTokenHashAsync(tokenHash);
        if (token is null)
        {
            throw new TokenNotFoundException();
        }

        if (token.IsExpired())
        {
            throw new TokenExpiredException();
        }

        if (!token.IsMultiUse)
        {
            var usedAt = _dateTimeProvider.UtcNow;
            var markedAsUsed = await _downloadTokenRepository.TryMarkAsUsedAsync(new TokenId(token.Id), usedAt, cancellationToken);
            if (!markedAsUsed)
            {
                var currentToken = await _downloadTokenRepository.GetByIdAsync(new TokenId(token.Id));
                if (currentToken is null)
                {
                    throw new TokenNotFoundException();
                }

                if (currentToken.IsExpired())
                {
                    throw new TokenExpiredException();
                }

                if (currentToken.IsRevoked)
                {
                    throw new TokenRevokedException();
                }

                throw new TokenAlreadyUsedException();
            }
        }

        var statement = await _statementRepository.GetByIdAsync(token.StatementId);
        if (statement is null)
        {
            throw new StatementNotFoundException();
        }

        var stream = await _fileStorage.DownloadAsync(statement.StoragePath);
        await _auditLogRepository.AddAsync(new AuditLog(
            Guid.NewGuid(),
            token.StatementId,
            new TokenId(token.Id),
            AuditAction.TokenRedeemed,
            _dateTimeProvider.UtcNow,
            request.RedeemedBy,
            request.IpAddress,
            $"Statement '{statement.FileName}' downloaded."));

        return new StatementFileResult(stream, statement.FileName, statement.ContentType);
    }
}
