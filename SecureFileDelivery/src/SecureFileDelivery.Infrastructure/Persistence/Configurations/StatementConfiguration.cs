using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Infrastructure.Persistence.Configurations;

public sealed class StatementConfiguration : IEntityTypeConfiguration<Statement>
{
    public void Configure(EntityTypeBuilder<Statement> builder)
    {
        builder.ToTable("Statements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerId)
            .HasConversion(x => x.Value, x => new CustomerId(x))
            .IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.CustomerId);
    }
}
