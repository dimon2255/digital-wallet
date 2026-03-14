using BitcoinPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BitcoinPayments.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures wallet persistence.
/// </summary>
public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");
        builder.HasKey(wallet => wallet.Id);

        builder.Property(wallet => wallet.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(wallet => wallet.EncryptedMasterKey)
            .HasColumnName("encrypted_master_key")
            .IsRequired();

        builder.Property(wallet => wallet.CurrentReceivingIndex)
            .HasColumnName("current_receiving_index")
            .IsRequired();

        builder.Property(wallet => wallet.CurrentChangeIndex)
            .HasColumnName("current_change_index")
            .IsRequired();

        builder.Property(wallet => wallet.Network)
            .HasColumnName("network")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(wallet => wallet.CreatedAt).HasColumnName("created_at");
        builder.Property(wallet => wallet.UpdatedAt).HasColumnName("updated_at");
    }
}
