using MediatR;
using SecureFileDelivery.Application.Common.Mappings;
using SecureFileDelivery.Application.DTOs;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Queries.GetStatementsByCustomer;

public sealed class GetStatementsByCustomerQueryHandler : IRequestHandler<GetStatementsByCustomerQuery, PagedResult<StatementDto>>
{
    private readonly IStatementRepository _statementRepository;

    public GetStatementsByCustomerQueryHandler(IStatementRepository statementRepository)
    {
        _statementRepository = statementRepository;
    }

    public async Task<PagedResult<StatementDto>> Handle(GetStatementsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _statementRepository.GetByCustomerIdAsync(new CustomerId(request.CustomerId), request.Page, request.PageSize);
        var statements = items.Select(x => x.ToDto()).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        return new PagedResult<StatementDto>(statements, request.Page, request.PageSize, totalCount, totalPages);
    }
}
