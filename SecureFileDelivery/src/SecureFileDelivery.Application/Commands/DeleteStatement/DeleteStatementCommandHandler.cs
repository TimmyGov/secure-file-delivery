using MediatR;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Commands.DeleteStatement;

public sealed class DeleteStatementCommandHandler : IRequestHandler<DeleteStatementCommand, bool>
{
    private readonly IStatementRepository _statementRepository;
    private readonly IFileStorage _fileStorage;

    public DeleteStatementCommandHandler(IStatementRepository statementRepository, IFileStorage fileStorage)
    {
        _statementRepository = statementRepository;
        _fileStorage = fileStorage;
    }

    public async Task<bool> Handle(DeleteStatementCommand request, CancellationToken cancellationToken)
    {
        var statement = await _statementRepository.GetByIdAsync(new StatementId(request.StatementId));
        if (statement is null)
        {
            throw new StatementNotFoundException();
        }

        statement.MarkAsDeleted();
        await _fileStorage.DeleteAsync(statement.StoragePath);
        await _statementRepository.UpdateAsync(statement);
        return true;
    }
}
