using MediatR;
using SecureFileDelivery.Application.DTOs;

namespace SecureFileDelivery.Application.Queries.GetStatementsByCustomer;

public sealed record GetStatementsByCustomerQuery(Guid CustomerId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<StatementDto>>;
