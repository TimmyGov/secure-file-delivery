using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Enums;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;
using SecureFileDelivery.Infrastructure.Persistence;

namespace SecureFileDelivery.Infrastructure.Services;

public sealed class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TokenCleanupSettings _settings;

    public TokenCleanupService(IServiceProvider serviceProvider, IOptions<TokenCleanupSettings> options, ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token cleanup cycle failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(Math.Max(1, _settings.IntervalMinutes)), stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenRepository = scope.ServiceProvider.GetRequiredService<IDownloadTokenRepository>();
        var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

        var expiredTokens = await dbContext.DownloadTokens
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var deletedCount = await tokenRepository.DeleteExpiredAsync();

        foreach (var token in expiredTokens)
        {
            await auditLogRepository.AddAsync(new AuditLog(
                Guid.NewGuid(),
                token.StatementId,
                new TokenId(token.Id),
                AuditAction.TokenExpired,
                DateTime.UtcNow,
                "system-cleanup",
                "127.0.0.1",
                "Expired download token removed by cleanup service."));
        }

        if (deletedCount > 0)
        {
            _logger.LogInformation("Deleted {DeletedCount} expired download tokens.", deletedCount);
        }
    }
}
