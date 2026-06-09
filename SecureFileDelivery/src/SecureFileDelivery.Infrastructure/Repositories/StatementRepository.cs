using Microsoft.EntityFrameworkCore;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;
using SecureFileDelivery.Infrastructure.Persistence;

namespace SecureFileDelivery.Infrastructure.Repositories;

public sealed class StatementRepository : IStatementRepository
{
    private readonly AppDbContext _dbContext;

    public StatementRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Statement?> GetByIdAsync(StatementId id) =>
        await _dbContext.Statements.FirstOrDefaultAsync(x => x.Id == id.Value && !x.IsDeleted);

    public async Task<(IEnumerable<Statement> Items, int TotalCount)> GetByCustomerIdAsync(CustomerId customerId, int page, int pageSize)
    {
        var query = _dbContext.Statements
            .Where(x => x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.UploadedAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task AddAsync(Statement statement)
    {
        await _dbContext.Statements.AddAsync(statement);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Statement statement)
    {
        _dbContext.Statements.Update(statement);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(StatementId id)
    {
        var statement = await _dbContext.Statements.FirstOrDefaultAsync(x => x.Id == id.Value);
        if (statement is null)
        {
            return;
        }

        _dbContext.Statements.Remove(statement);
        await _dbContext.SaveChangesAsync();
    }
}
