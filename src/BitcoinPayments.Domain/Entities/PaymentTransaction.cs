using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Domain.Entities;

/// <summary>
/// Represents a payment operation tracked by the platform.
/// </summary>
public class PaymentTransaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentTransaction"/> class.
    /// </summary>
    public PaymentTransaction()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentTransaction"/> class.
    /// </summary>
    /// <param name="idempotencyKey">The idempotency key.</param>
    /// <param name="operationType">The operation type.</param>
    /// <param name="amount">The requested amount.</param>
    /// <param name="buyerWalletId">The buyer wallet.</param>
    /// <param name="merchantWalletId">The merchant wallet.</param>
    public PaymentTransaction(
        string? idempotencyKey,
        PaymentOperationType operationType,
        Money amount,
        Guid buyerWalletId,
        Guid merchantWalletId)
    {
        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey;
        OperationType = operationType;
        State = TransactionState.Created;
        Amount = amount;
        BuyerWalletId = buyerWalletId;
        MerchantWalletId = merchantWalletId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the idempotency key.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// Gets or sets the operation type.
    /// </summary>
    public PaymentOperationType OperationType { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle state.
    /// </summary>
    public TransactionState State { get; set; }

    /// <summary>
    /// Gets or sets the requested amount.
    /// </summary>
    public Money Amount { get; set; }

    /// <summary>
    /// Gets or sets the paid network fee.
    /// </summary>
    public Money? Fee { get; set; }

    /// <summary>
    /// Gets or sets the buyer wallet identifier.
    /// </summary>
    public Guid BuyerWalletId { get; set; }

    /// <summary>
    /// Gets or sets the merchant wallet identifier.
    /// </summary>
    public Guid MerchantWalletId { get; set; }

    /// <summary>
    /// Gets or sets the on-chain transaction id.
    /// </summary>
    public TransactionId? BitcoinTxId { get; set; }

    /// <summary>
    /// Gets or sets the raw transaction hex.
    /// </summary>
    public string? RawTransactionHex { get; set; }

    /// <summary>
    /// Gets or sets the confirmation count.
    /// </summary>
    public int ConfirmationCount { get; set; }

    /// <summary>
    /// Gets or sets the parent operation identifier.
    /// </summary>
    public Guid? ParentTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the JSON-serializable metadata.
    /// </summary>
    public Dictionary<string, string?> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets the error message for failed operations.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Marks the transaction as broadcast and stores its raw data.
    /// </summary>
    /// <param name="transactionId">The on-chain transaction id.</param>
    /// <param name="rawTransactionHex">The raw transaction hex.</param>
    /// <param name="fee">The fee paid.</param>
    /// <param name="state">The post-broadcast state.</param>
    public void MarkBroadcast(TransactionId transactionId, string rawTransactionHex, Money fee, TransactionState state)
    {
        BitcoinTxId = transactionId;
        RawTransactionHex = rawTransactionHex;
        Fee = fee;
        State = state;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates confirmation tracking and transitions to settlement when appropriate.
    /// </summary>
    /// <param name="confirmationCount">The confirmation count.</param>
    /// <param name="settlementConfirmations">The confirmations required for settlement.</param>
    public void UpdateConfirmations(int confirmationCount, int settlementConfirmations)
    {
        ConfirmationCount = confirmationCount;

        if (confirmationCount <= 0)
        {
            State = OperationType == PaymentOperationType.Auth ? TransactionState.FundingBroadcast : TransactionState.Mempool;
        }
        else if (OperationType == PaymentOperationType.Auth)
        {
            State = TransactionState.AuthActive;
        }
        else if (confirmationCount >= settlementConfirmations)
        {
            State = State == TransactionState.VoidBroadcast || State == TransactionState.VoidConfirming
                ? TransactionState.Voided
                : TransactionState.Settled;
        }
        else
        {
            State = State == TransactionState.VoidBroadcast ? TransactionState.VoidConfirming : TransactionState.Confirming;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the transaction as failed.
    /// </summary>
    /// <param name="message">The failure reason.</param>
    public void MarkFailed(string message)
    {
        State = TransactionState.Failed;
        ErrorMessage = message;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Sets the parent transaction link.
    /// </summary>
    /// <param name="parentTransactionId">The parent transaction id.</param>
    public void AttachToParent(Guid parentTransactionId)
    {
        ParentTransactionId = parentTransactionId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Transitions the transaction to a specific state.
    /// </summary>
    /// <param name="state">The next state.</param>
    public void TransitionTo(TransactionState state)
    {
        if (State == TransactionState.Settled || State == TransactionState.Voided || State == TransactionState.Expired)
        {
            throw new InvalidOperationStateException($"Transaction {Id} is already terminal and cannot move to {state}.");
        }

        State = state;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
