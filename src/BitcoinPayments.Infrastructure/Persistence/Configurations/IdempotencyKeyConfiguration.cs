using BitcoinPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BitcoinPayments.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures idempotency key persistence.
/// </summary>
public sealed class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKeyRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<IdempotencyKeyRecord> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(record => record.Key);
        builder.HasIndex(record => record.ExpiresAt).HasDatabaseName("idx_idempotency_expires");

        builder.Property(record => record.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
        builder.Property(record => record.ResponseStatusCode).HasColumnName("response_status_code").IsRequired();
        builder.Property(record => record.ResponseBody).HasColumnName("response_body").IsRequired();
        builder.Property(record => record.CreatedAt).HasColumnName("created_at");
        builder.Property(record => record.ExpiresAt).HasColumnName("expires_at");
    }
}
