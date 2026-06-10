using MediatR;
using SecureFileDelivery.Application.Common.Mappings;
using SecureFileDelivery.Application.DTOs;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Enums;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Commands.GenerateDownloadToken;

public sealed class GenerateDownloadTokenCommandHandler : IRequestHandler<GenerateDownloadTokenCommand, DownloadTokenDto>
{
    private readonly IStatementRepository _statementRepository;
    private readonly IDownloadTokenRepository _downloadTokenRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GenerateDownloadTokenCommandHandler(
        IStatementRepository statementRepository,
        IDownloadTokenRepository downloadTokenRepository,
        IAuditLogRepository auditLogRepository,
        ITokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IDateTimeProvider dateTimeProvider)
    {
        _statementRepository = statementRepository;
        _downloadTokenRepository = downloadTokenRepository;
        _auditLogRepository = auditLogRepository;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DownloadTokenDto> Handle(GenerateDownloadTokenCommand request, CancellationToken cancellationToken)
    {
        var statement = await _statementRepository.GetByIdAsync(new StatementId(request.StatementId));
        if (statement is null)
        {
            throw new StatementNotFoundException();
        }

        var rawToken = _tokenGenerator.Generate();
        var token = new DownloadToken(
            Guid.NewGuid(),
            new StatementId(request.StatementId),
            _tokenHasher.Hash(rawToken),
            _dateTimeProvider.UtcNow.AddMinutes(request.TtlMinutes),
            _dateTimeProvider.UtcNow,
            request.IsMultiUse);

        await _downloadTokenRepository.AddAsync(token);
        await _auditLogRepository.AddAsync(new AuditLog(
            Guid.NewGuid(),
            new StatementId(request.StatementId),
            new TokenId(token.Id),
            AuditAction.TokenGenerated,
            _dateTimeProvider.UtcNow,
            request.RequestedBy,
            "n/a",
            $"Download token generated with TTL of {request.TtlMinutes} minutes."));

        return token.ToDto(rawToken);
    }
}
