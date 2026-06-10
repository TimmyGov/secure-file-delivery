using MediatR;
using SecureFileDelivery.Application.Common.Mappings;
using SecureFileDelivery.Application.DTOs;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Enums;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Commands.UploadStatement;

public sealed class UploadStatementCommandHandler : IRequestHandler<UploadStatementCommand, StatementDto>
{
    private readonly IStatementRepository _statementRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UploadStatementCommandHandler(
        IStatementRepository statementRepository,
        IAuditLogRepository auditLogRepository,
        IFileStorage fileStorage,
        IDateTimeProvider dateTimeProvider)
    {
        _statementRepository = statementRepository;
        _auditLogRepository = auditLogRepository;
        _fileStorage = fileStorage;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<StatementDto> Handle(UploadStatementCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidFileTypeException();
        }

        var storagePath = await _fileStorage.UploadAsync(request.FileStream, request.FileName, request.ContentType);
        var statement = new Statement(
            Guid.NewGuid(),
            new CustomerId(request.CustomerId),
            request.FileName,
            storagePath,
            request.FileSizeBytes,
            request.ContentType,
            _dateTimeProvider.UtcNow);

        await _statementRepository.AddAsync(statement);
        await _auditLogRepository.AddAsync(new AuditLog(
            Guid.NewGuid(),
            new StatementId(statement.Id),
            null,
            AuditAction.Uploaded,
            _dateTimeProvider.UtcNow,
            "system",
            "n/a",
            $"Statement '{statement.FileName}' uploaded."));

        return statement.ToDto();
    }
}
