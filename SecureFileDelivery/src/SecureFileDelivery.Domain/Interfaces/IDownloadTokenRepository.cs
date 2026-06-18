using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Interfaces;

public interface IDownloadTokenRepository
{
    Task<DownloadToken?> GetByTokenHashAsync(string hash);
    Task<DownloadToken?> GetByIdAsync(TokenId id);
    Task AddAsync(DownloadToken token);
    Task UpdateAsync(DownloadToken token);
    Task<bool> TryMarkAsUsedAsync(TokenId id, DateTime usedAt, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredAsync();
}
