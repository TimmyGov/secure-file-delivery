using MediatR;
using SecureFileDelivery.Application.DTOs;

namespace SecureFileDelivery.Application.Queries.GetStatementById;

public sealed record GetStatementByIdQuery(Guid StatementId) : IRequest<StatementDto>;
