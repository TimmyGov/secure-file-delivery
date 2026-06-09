using MediatR;
using SecureFileDelivery.Application.Common.Mappings;
using SecureFileDelivery.Application.DTOs;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Queries.GetAuditLogsForStatement;

public sealed class GetAuditLogsForStatementQueryHandler : IRequestHandler<GetAuditLogsForStatementQuery, PagedResult<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsForStatementQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsForStatementQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _auditLogRepository.GetByStatementIdAsync(new StatementId(request.StatementId), request.Page, request.PageSize);
        var logs = items.Select(x => x.ToDto()).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        return new PagedResult<AuditLogDto>(logs, request.Page, request.PageSize, totalCount, totalPages);
    }
}
