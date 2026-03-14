using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Domain.Entities;

/// <summary>
/// Represents an authorization escrow.
/// </summary>
public class Escrow
{
    /// <summary>
    /// Gets or sets the escrow identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the parent payment transaction identifier.
    /// </summary>
    public Guid PaymentTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the escrow address.
    /// </summary>
    public BitcoinAddress EscrowAddress { get; set; }

    /// <summary>
    /// Gets or sets the redeem script hex.
    /// </summary>
    public string RedeemScriptHex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the buyer public key hex.
    /// </summary>
    public string BuyerPublicKeyHex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the merchant public key hex.
    /// </summary>
    public string MerchantPublicKeyHex { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timelock window in blocks.
    /// </summary>
    public int TimelockBlocks { get; set; }

    /// <summary>
    /// Gets or sets the prebuilt refund transaction hex.
    /// </summary>
    public string? TimelockRefundTransactionHex { get; set; }

    /// <summary>
    /// Gets or sets the funded escrow output identifier.
    /// </summary>
    public Guid? FundingUtxoId { get; set; }

    /// <summary>
    /// Gets or sets the buyer escrow key derivation index.
    /// </summary>
    public int BuyerKeyIndex { get; set; }

    /// <summary>
    /// Gets or sets the merchant escrow key derivation index.
    /// </summary>
    public int MerchantKeyIndex { get; set; }

    /// <summary>
    /// Gets or sets the buyer change derivation index used for funding.
    /// </summary>
    public int BuyerChangeIndex { get; set; }

    /// <summary>
    /// Gets or sets the expiration block height.
    /// </summary>
    public long? ExpiresAtBlock { get; set; }

    /// <summary>
    /// Gets or sets the escrow state.
    /// </summary>
    public EscrowState State { get; set; } = EscrowState.Created;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Marks the escrow as funded.
    /// </summary>
    /// <param name="fundingUtxoId">The escrow funding UTXO identifier.</param>
    /// <param name="expiresAtBlock">The expiration block height.</param>
    public void MarkFunded(Guid fundingUtxoId, long expiresAtBlock)
    {
        FundingUtxoId = fundingUtxoId;
        ExpiresAtBlock = expiresAtBlock;
        State = EscrowState.Funded;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the escrow as captured.
    /// </summary>
    public void MarkCaptured()
    {
        State = EscrowState.Captured;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the escrow as voided.
    /// </summary>
    public void MarkVoided()
    {
        State = EscrowState.Voided;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the escrow as expired.
    /// </summary>
    public void MarkExpired()
    {
        State = EscrowState.Expired;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
