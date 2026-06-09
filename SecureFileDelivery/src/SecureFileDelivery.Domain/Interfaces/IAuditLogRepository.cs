using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetByStatementIdAsync(StatementId statementId, int page, int pageSize);
}
