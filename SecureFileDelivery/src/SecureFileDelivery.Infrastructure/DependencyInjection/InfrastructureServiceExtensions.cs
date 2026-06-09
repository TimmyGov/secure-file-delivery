using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Infrastructure.Persistence;
using SecureFileDelivery.Infrastructure.Repositories;
using SecureFileDelivery.Infrastructure.Security;
using SecureFileDelivery.Infrastructure.Services;
using SecureFileDelivery.Infrastructure.Storage;

namespace SecureFileDelivery.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioSettings>(configuration.GetSection("Storage:Minio"));
        services.Configure<LocalStorageSettings>(configuration.GetSection("Storage:Local"));
        services.Configure<TokenCleanupSettings>(configuration.GetSection("TokenCleanup"));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        var databaseProvider = configuration["DatabaseProvider"];

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "Sqlite", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IStatementRepository, StatementRepository>();
        services.AddScoped<IDownloadTokenRepository, DownloadTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddSingleton<IMinioClient>(sp =>
        {
            var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MinioSettings>>().Value;
            return new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithSSL(settings.UseSSL)
                .Build();
        });

        services.AddScoped<IFileStorage>(sp =>
        {
            var provider = configuration["Storage:Provider"] ?? "Local";
            return string.Equals(provider, "Minio", StringComparison.OrdinalIgnoreCase)
                ? new MinioFileStorage(sp.GetRequiredService<IMinioClient>(), sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MinioSettings>>())
                : new LocalFileStorage(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<LocalStorageSettings>>());
        });

        services.AddSingleton<ITokenGenerator, TokenGenerator>();
        services.AddSingleton<ITokenHasher, Sha256TokenHasher>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddHostedService<TokenCleanupService>();

        return services;
    }
}
