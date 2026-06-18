using Microsoft.EntityFrameworkCore;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;
using SecureFileDelivery.Infrastructure.Persistence;

namespace SecureFileDelivery.Infrastructure.Repositories;

public sealed class DownloadTokenRepository : IDownloadTokenRepository
{
    private readonly AppDbContext _dbContext;

    public DownloadTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DownloadToken?> GetByTokenHashAsync(string hash) =>
        await _dbContext.DownloadTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);

    public async Task<DownloadToken?> GetByIdAsync(TokenId id) =>
        await _dbContext.DownloadTokens.FirstOrDefaultAsync(x => x.Id == id.Value);

    public async Task AddAsync(DownloadToken token)
    {
        await _dbContext.DownloadTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(DownloadToken token)
    {
        _dbContext.DownloadTokens.Update(token);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> TryMarkAsUsedAsync(TokenId id, DateTime usedAt, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbContext.DownloadTokens
            .Where(x => x.Id == id.Value && !x.IsRevoked && x.UsedAt == null && x.ExpiresAt >= usedAt)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.UsedAt, usedAt), cancellationToken);

        return rowsAffected > 0;
    }

    public async Task<int> DeleteExpiredAsync()
    {
        var expiredTokens = await _dbContext.DownloadTokens.Where(x => x.ExpiresAt < DateTime.UtcNow).ToListAsync();
        if (expiredTokens.Count == 0)
        {
            return 0;
        }

        _dbContext.DownloadTokens.RemoveRange(expiredTokens);
        await _dbContext.SaveChangesAsync();
        return expiredTokens.Count;
    }
}
