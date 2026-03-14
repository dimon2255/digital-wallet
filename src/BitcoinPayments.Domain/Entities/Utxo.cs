using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Domain.Entities;

/// <summary>
/// Represents a tracked unspent transaction output.
/// </summary>
public class Utxo
{
    /// <summary>
    /// Gets or sets the database identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning wallet identifier.
    /// </summary>
    public Guid WalletId { get; set; }

    /// <summary>
    /// Gets or sets the Bitcoin transaction id.
    /// </summary>
    public TransactionId TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the output index.
    /// </summary>
    public int OutputIndex { get; set; }

    /// <summary>
    /// Gets or sets the UTXO value.
    /// </summary>
    public Money Amount { get; set; }

    /// <summary>
    /// Gets or sets the script pubkey hex.
    /// </summary>
    public string ScriptPubKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the receiving address.
    /// </summary>
    public BitcoinAddress Address { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the output is spent.
    /// </summary>
    public bool IsSpent { get; set; }

    /// <summary>
    /// Gets or sets the spending transaction id.
    /// </summary>
    public TransactionId? SpentByTx { get; set; }

    /// <summary>
    /// Gets or sets the confirmation count.
    /// </summary>
    public int ConfirmationCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the output belongs to escrow.
    /// </summary>
    public bool IsEscrow { get; set; }

    /// <summary>
    /// Gets or sets the derivation index for the tracked address.
    /// </summary>
    public int DerivationIndex { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tracked address is from the change chain.
    /// </summary>
    public bool IsChangeAddress { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Marks the UTXO as spent.
    /// </summary>
    /// <param name="spendingTransactionId">The spending transaction id.</param>
    public void MarkSpent(TransactionId spendingTransactionId)
    {
        IsSpent = true;
        SpentByTx = spendingTransactionId;
    }

    /// <summary>
    /// Updates the confirmation count.
    /// </summary>
    /// <param name="confirmationCount">The confirmation count.</param>
    public void UpdateConfirmations(int confirmationCount) => ConfirmationCount = confirmationCount;
}
