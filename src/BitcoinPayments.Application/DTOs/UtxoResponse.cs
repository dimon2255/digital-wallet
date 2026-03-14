namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a tracked UTXO response.
/// </summary>
/// <param name="Id">The internal identifier.</param>
/// <param name="TransactionId">The transaction id.</param>
/// <param name="OutputIndex">The output index.</param>
/// <param name="AmountSatoshis">The amount in satoshis.</param>
/// <param name="Address">The address string.</param>
/// <param name="ConfirmationCount">The confirmation count.</param>
/// <param name="IsSpent">Whether the UTXO is spent.</param>
/// <param name="IsEscrow">Whether the UTXO is an escrow output.</param>
public sealed record UtxoResponse(
    Guid Id,
    string TransactionId,
    int OutputIndex,
    long AmountSatoshis,
    string Address,
    int ConfirmationCount,
    bool IsSpent,
    bool IsEscrow);
