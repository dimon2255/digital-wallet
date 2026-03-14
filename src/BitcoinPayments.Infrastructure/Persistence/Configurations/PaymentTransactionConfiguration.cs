using BitcoinPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BitcoinPayments.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures payment transaction persistence.
/// </summary>
public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transactions");
        builder.HasKey(transaction => transaction.Id);
        builder.HasIndex(transaction => transaction.IdempotencyKey).IsUnique();
        builder.HasIndex(transaction => transaction.State).HasDatabaseName("idx_payment_tx_state");
        builder.HasIndex(transaction => transaction.BitcoinTxId).HasDatabaseName("idx_payment_tx_bitcoin_txid");
        builder.HasIndex(transaction => transaction.ParentTransactionId).HasDatabaseName("idx_payment_tx_parent");

        builder.Property(transaction => transaction.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(100);
        builder.Property(transaction => transaction.OperationType)
            .HasColumnName("operation_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(transaction => transaction.State)
            .HasColumnName("state")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(transaction => transaction.Amount)
            .HasColumnName("amount_satoshis")
            .HasConversion(EntityConfigurationHelpers.MoneyConverter)
            .IsRequired();
        builder.Property(transaction => transaction.Fee)
            .HasColumnName("fee_satoshis")
            .HasConversion(EntityConfigurationHelpers.NullableMoneyConverter);
        builder.Property(transaction => transaction.BuyerWalletId).HasColumnName("buyer_wallet_id").IsRequired();
        builder.Property(transaction => transaction.MerchantWalletId).HasColumnName("merchant_wallet_id").IsRequired();
        builder.Property(transaction => transaction.BitcoinTxId)
            .HasColumnName("bitcoin_tx_id")
            .HasConversion(EntityConfigurationHelpers.NullableTransactionIdConverter)
            .HasMaxLength(64);
        builder.Property(transaction => transaction.RawTransactionHex).HasColumnName("raw_transaction_hex");
        builder.Property(transaction => transaction.ConfirmationCount).HasColumnName("confirmation_count").IsRequired();
        builder.Property(transaction => transaction.ParentTransactionId).HasColumnName("parent_transaction_id");

        var metadataProperty = builder.Property(transaction => transaction.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(EntityConfigurationHelpers.MetadataConverter);
        metadataProperty.Metadata.SetValueComparer(EntityConfigurationHelpers.MetadataComparer);

        builder.Property(transaction => transaction.ErrorMessage).HasColumnName("error_message");
        builder.Property(transaction => transaction.CreatedAt).HasColumnName("created_at");
        builder.Property(transaction => transaction.UpdatedAt).HasColumnName("updated_at");
    }
}
