using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Domain.Interfaces;

/// <summary>
/// Provides network operations against the configured Bitcoin environment.
/// </summary>
public interface IBitcoinNetwork
{
    /// <summary>
    /// Broadcasts a raw transaction.
    /// </summary>
    Task<BroadcastResult> BroadcastTransactionAsync(string rawTransactionHex, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves tracked UTXOs for a set of addresses.
    /// </summary>
    Task<IReadOnlyCollection<ObservedUtxo>> GetAddressUtxosAsync(
        IReadOnlyCollection<BitcoinAddress> addresses,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the status for an on-chain transaction.
    /// </summary>
    Task<NetworkTransactionStatus> GetTransactionStatusAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the current block height.
    /// </summary>
    Task<long> GetCurrentBlockHeightAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Funds an address in regtest mode.
    /// </summary>
    Task<FundingResult> FundAddressAsync(BitcoinAddress address, Money amount, CancellationToken cancellationToken);
}

/// <summary>
/// Represents the result of broadcasting a transaction.
/// </summary>
/// <param name="TransactionId">The transaction id.</param>
/// <param name="ExplorerUrl">An optional explorer URL.</param>
public sealed record BroadcastResult(TransactionId TransactionId, string? ExplorerUrl);

/// <summary>
/// Represents a network-observed UTXO.
/// </summary>
/// <param name="TransactionId">The transaction id.</param>
/// <param name="OutputIndex">The output index.</param>
/// <param name="Amount">The output amount.</param>
/// <param name="ScriptPubKey">The script pubkey.</param>
/// <param name="Address">The output address.</param>
/// <param name="ConfirmationCount">The confirmation count.</param>
public sealed record ObservedUtxo(
    TransactionId TransactionId,
    int OutputIndex,
    Money Amount,
    string ScriptPubKey,
    BitcoinAddress Address,
    int ConfirmationCount);

/// <summary>
/// Represents the observed status of an on-chain transaction.
/// </summary>
/// <param name="TransactionId">The transaction id.</param>
/// <param name="InMempool">Whether the transaction is in the mempool.</param>
/// <param name="ConfirmationCount">The confirmation count.</param>
/// <param name="Confirmed">Whether the transaction is confirmed.</param>
public sealed record NetworkTransactionStatus(
    TransactionId TransactionId,
    bool InMempool,
    int ConfirmationCount,
    bool Confirmed);

/// <summary>
/// Represents the result of a development funding operation.
/// </summary>
/// <param name="TransactionId">The resulting transaction id, when available.</param>
/// <param name="Message">The operator-facing message.</param>
public sealed record FundingResult(TransactionId? TransactionId, string Message);
