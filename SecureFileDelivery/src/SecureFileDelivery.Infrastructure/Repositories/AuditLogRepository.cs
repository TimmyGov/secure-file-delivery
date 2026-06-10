using Microsoft.EntityFrameworkCore;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;
using SecureFileDelivery.Infrastructure.Persistence;

namespace SecureFileDelivery.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _dbContext;

    public AuditLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog log)
    {
        await _dbContext.AuditLogs.AddAsync(log);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetByStatementIdAsync(StatementId statementId, int page, int pageSize)
    {
        var query = _dbContext.AuditLogs
            .Where(x => x.StatementId == statementId)
            .OrderByDescending(x => x.PerformedAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }
}
