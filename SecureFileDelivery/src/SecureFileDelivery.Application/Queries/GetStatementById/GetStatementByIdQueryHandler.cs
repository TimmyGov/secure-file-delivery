using MediatR;
using SecureFileDelivery.Application.Common.Mappings;
using SecureFileDelivery.Application.DTOs;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Queries.GetStatementById;

public sealed class GetStatementByIdQueryHandler : IRequestHandler<GetStatementByIdQuery, StatementDto>
{
    private readonly IStatementRepository _statementRepository;

    public GetStatementByIdQueryHandler(IStatementRepository statementRepository)
    {
        _statementRepository = statementRepository;
    }

    public async Task<StatementDto> Handle(GetStatementByIdQuery request, CancellationToken cancellationToken)
    {
        var statement = await _statementRepository.GetByIdAsync(new StatementId(request.StatementId));
        if (statement is null)
        {
            throw new StatementNotFoundException();
        }

        return statement.ToDto();
    }
}
