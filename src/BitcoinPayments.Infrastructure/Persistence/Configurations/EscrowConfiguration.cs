using BitcoinPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BitcoinPayments.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures escrow persistence.
/// </summary>
public sealed class EscrowConfiguration : IEntityTypeConfiguration<Escrow>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Escrow> builder)
    {
        builder.ToTable("escrows");
        builder.HasKey(escrow => escrow.Id);

        builder.Property(escrow => escrow.PaymentTransactionId).HasColumnName("payment_transaction_id").IsRequired();
        builder.Property(escrow => escrow.EscrowAddress)
            .HasColumnName("escrow_address")
            .HasConversion(EntityConfigurationHelpers.BitcoinAddressConverter)
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(escrow => escrow.RedeemScriptHex).HasColumnName("redeem_script_hex").IsRequired();
        builder.Property(escrow => escrow.BuyerPublicKeyHex).HasColumnName("buyer_pubkey_hex").HasMaxLength(66).IsRequired();
        builder.Property(escrow => escrow.MerchantPublicKeyHex).HasColumnName("merchant_pubkey_hex").HasMaxLength(66).IsRequired();
        builder.Property(escrow => escrow.TimelockBlocks).HasColumnName("timelock_blocks").IsRequired();
        builder.Property(escrow => escrow.TimelockRefundTransactionHex).HasColumnName("timelock_refund_tx_hex");
        builder.Property(escrow => escrow.FundingUtxoId).HasColumnName("funding_utxo_id");
        builder.Property(escrow => escrow.BuyerKeyIndex).HasColumnName("buyer_key_index").IsRequired();
        builder.Property(escrow => escrow.MerchantKeyIndex).HasColumnName("merchant_key_index").IsRequired();
        builder.Property(escrow => escrow.BuyerChangeIndex).HasColumnName("buyer_change_index").IsRequired();
        builder.Property(escrow => escrow.ExpiresAtBlock).HasColumnName("expires_at_block");
        builder.Property(escrow => escrow.State)
            .HasColumnName("state")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(escrow => escrow.CreatedAt).HasColumnName("created_at");
        builder.Property(escrow => escrow.UpdatedAt).HasColumnName("updated_at");
    }
}
