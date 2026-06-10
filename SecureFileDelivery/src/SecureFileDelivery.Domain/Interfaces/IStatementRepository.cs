using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Interfaces;

public interface IStatementRepository
{
    Task<Statement?> GetByIdAsync(StatementId id);
    Task<(IEnumerable<Statement> Items, int TotalCount)> GetByCustomerIdAsync(CustomerId customerId, int page, int pageSize);
    Task AddAsync(Statement statement);
    Task UpdateAsync(Statement statement);
    Task DeleteAsync(StatementId id);
}
