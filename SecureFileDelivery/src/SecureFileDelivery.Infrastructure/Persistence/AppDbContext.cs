using Microsoft.EntityFrameworkCore;
using SecureFileDelivery.Domain.Entities;

namespace SecureFileDelivery.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Statement> Statements => Set<Statement>();
    public DbSet<DownloadToken> DownloadTokens => Set<DownloadToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
