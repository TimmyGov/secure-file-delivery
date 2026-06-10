using MediatR;
using SecureFileDelivery.Application.DTOs;

namespace SecureFileDelivery.Application.Queries.GetAuditLogsForStatement;

public sealed record GetAuditLogsForStatementQuery(Guid StatementId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<AuditLogDto>>;
