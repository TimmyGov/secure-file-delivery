using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Enums;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StatementId)
            .HasConversion(x => x.Value, x => new StatementId(x))
            .IsRequired();
        builder.Property(x => x.TokenId)
            .HasConversion(
                x => x.HasValue ? x.Value.Value : (Guid?)null,
                x => x.HasValue ? new TokenId(x.Value) : (TokenId?)null);
        builder.Property(x => x.Action)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(x => x.PerformedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => x.StatementId);
    }
}
