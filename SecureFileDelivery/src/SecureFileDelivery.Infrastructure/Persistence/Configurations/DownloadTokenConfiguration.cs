using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Infrastructure.Persistence.Configurations;

public sealed class DownloadTokenConfiguration : IEntityTypeConfiguration<DownloadToken>
{
    public void Configure(EntityTypeBuilder<DownloadToken> builder)
    {
        builder.ToTable("DownloadTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StatementId)
            .HasConversion(x => x.Value, x => new StatementId(x))
            .IsRequired();
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
