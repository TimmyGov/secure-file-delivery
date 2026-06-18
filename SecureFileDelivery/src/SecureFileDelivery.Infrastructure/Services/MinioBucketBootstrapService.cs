using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SecureFileDelivery.Infrastructure.Storage;

namespace SecureFileDelivery.Infrastructure.Services;

public sealed class MinioBucketBootstrapService : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioBucketBootstrapService> _logger;

    public MinioBucketBootstrapService(
        IConfiguration configuration,
        IMinioClient minioClient,
        IOptions<MinioSettings> options,
        ILogger<MinioBucketBootstrapService> logger)
    {
        _configuration = configuration;
        _minioClient = minioClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var provider = _configuration["Storage:Provider"] ?? "Local";
        if (!string.Equals(provider, "Minio", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName), cancellationToken);
        if (!exists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName), cancellationToken);
            _logger.LogInformation("Created MinIO bucket {BucketName} during application startup.", _settings.BucketName);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}