using BitcoinPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BitcoinPayments.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures UTXO persistence.
/// </summary>
public sealed class UtxoConfiguration : IEntityTypeConfiguration<Utxo>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Utxo> builder)
    {
        builder.ToTable("utxos");
        builder.HasKey(utxo => utxo.Id);
        builder.HasIndex(utxo => new { utxo.TransactionId, utxo.OutputIndex }).IsUnique();
        builder.HasIndex(utxo => new { utxo.WalletId, utxo.IsSpent })
            .HasDatabaseName("idx_utxos_wallet_unspent");

        builder.Property(utxo => utxo.WalletId).HasColumnName("wallet_id").IsRequired();
        builder.Property(utxo => utxo.TransactionId)
            .HasColumnName("transaction_id")
            .HasConversion(EntityConfigurationHelpers.TransactionIdConverter)
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(utxo => utxo.OutputIndex).HasColumnName("output_index").IsRequired();
        builder.Property(utxo => utxo.Amount)
            .HasColumnName("amount_satoshis")
            .HasConversion(EntityConfigurationHelpers.MoneyConverter)
            .IsRequired();
        builder.Property(utxo => utxo.ScriptPubKey).HasColumnName("script_pubkey").IsRequired();
        builder.Property(utxo => utxo.Address)
            .HasColumnName("address")
            .HasConversion(EntityConfigurationHelpers.BitcoinAddressConverter)
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(utxo => utxo.IsSpent).HasColumnName("is_spent").IsRequired();
        builder.Property(utxo => utxo.SpentByTx)
            .HasColumnName("spent_by_tx")
            .HasConversion(EntityConfigurationHelpers.NullableTransactionIdConverter);
        builder.Property(utxo => utxo.ConfirmationCount).HasColumnName("confirmation_count").IsRequired();
        builder.Property(utxo => utxo.IsEscrow).HasColumnName("is_escrow").IsRequired();
        builder.Property(utxo => utxo.DerivationIndex).HasColumnName("derivation_index").IsRequired();
        builder.Property(utxo => utxo.IsChangeAddress).HasColumnName("is_change_address").IsRequired();
        builder.Property(utxo => utxo.CreatedAt).HasColumnName("created_at");
    }
}
