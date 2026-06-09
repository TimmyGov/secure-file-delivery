using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SecureFileDelivery.Infrastructure.Storage;

namespace SecureFileDelivery.API.HealthChecks;

public sealed class StorageHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<LocalStorageSettings> _localOptions;
    private readonly IOptions<MinioSettings> _minioOptions;

    public StorageHealthCheck(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IOptions<LocalStorageSettings> localOptions,
        IOptions<MinioSettings> minioOptions)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _localOptions = localOptions;
        _minioOptions = minioOptions;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var provider = _configuration["Storage:Provider"] ?? "Local";
        if (string.Equals(provider, "Minio", StringComparison.OrdinalIgnoreCase))
        {
            var client = _serviceProvider.GetRequiredService<IMinioClient>();
            var settings = _minioOptions.Value;
            var exists = await client.BucketExistsAsync(new BucketExistsArgs().WithBucket(settings.BucketName), cancellationToken);
            return exists
                ? HealthCheckResult.Healthy("MinIO bucket is reachable.")
                : HealthCheckResult.Unhealthy("MinIO bucket does not exist.");
        }

        Directory.CreateDirectory(_localOptions.Value.BasePath);
        return Directory.Exists(_localOptions.Value.BasePath)
            ? HealthCheckResult.Healthy("Local storage is available.")
            : HealthCheckResult.Unhealthy("Local storage is unavailable.");
    }
}
