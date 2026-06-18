using MediatR;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Commands.DeleteStatement;

public sealed class DeleteStatementCommandHandler : IRequestHandler<DeleteStatementCommand, bool>
{
    private readonly IStatementRepository _statementRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStatementCommandHandler(IStatementRepository statementRepository, IFileStorage fileStorage, IUnitOfWork unitOfWork)
    {
        _statementRepository = statementRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteStatementCommand request, CancellationToken cancellationToken)
    {
        var statement = await _statementRepository.GetByIdAsync(new StatementId(request.StatementId));
        if (statement is null)
        {
            throw new StatementNotFoundException();
        }

        statement.MarkAsDeleted();
        await _unitOfWork.ExecuteInTransactionAsync(async _ =>
        {
            await _statementRepository.UpdateAsync(statement);
        });

        await _fileStorage.DeleteAsync(statement.StoragePath);
        return true;
    }
}
