using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Infrastructure.Persistence;
using SecureFileDelivery.Infrastructure.Security;

namespace SecureFileDelivery.Integration.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection _connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "Sqlite",
                ["ConnectionStrings:DefaultConnection"] = "Data Source=InMemoryTests;Mode=Memory;Cache=Shared",
                ["Storage:Provider"] = "Local",
                ["Storage:Local:BasePath"] = "integration-statements",
                ["Jwt:Issuer"] = "SecureFileDelivery",
                ["Jwt:Audience"] = "SecureFileDelivery",
                ["Jwt:SecretKey"] = "development-only-secret-change-me-32!!",
                ["TokenCleanup:IntervalMinutes"] = "1440"
            };
            configBuilder.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IFileStorage>();
            services.RemoveAll<ITokenGenerator>();
            services.RemoveAll<ITokenHasher>();
            services.RemoveAll<DbConnection>();

            _connection = new SqliteConnection("Data Source=InMemoryTests;Mode=Memory;Cache=Shared");
            _connection.Open();
            services.AddSingleton<DbConnection>(_connection);
            services.AddDbContext<AppDbContext>((sp, options) => options.UseSqlite(sp.GetRequiredService<DbConnection>()));
            services.AddSingleton<IFileStorage, InMemoryFileStorage>();
            services.AddSingleton<ITokenGenerator, TokenGenerator>();
            services.AddSingleton<ITokenHasher, Sha256TokenHasher>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
