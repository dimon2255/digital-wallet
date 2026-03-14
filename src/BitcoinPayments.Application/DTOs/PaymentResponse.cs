using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a payment operation response.
/// </summary>
/// <param name="Id">The internal payment identifier.</param>
/// <param name="OperationType">The operation type string.</param>
/// <param name="State">The current state string.</param>
/// <param name="AmountSatoshis">The amount in satoshis.</param>
/// <param name="FeeSatoshis">The paid fee in satoshis.</param>
/// <param name="BitcoinTxId">The Bitcoin transaction id.</param>
/// <param name="ConfirmationCount">The confirmation count.</param>
/// <param name="ExplorerUrl">The explorer URL.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
public sealed record PaymentResponse(
    Guid Id,
    string OperationType,
    string State,
    long AmountSatoshis,
    long? FeeSatoshis,
    string? BitcoinTxId,
    int ConfirmationCount,
    string? ExplorerUrl,
    DateTimeOffset CreatedAt)
{
    /// <summary>
    /// Creates a response from a payment entity.
    /// </summary>
    public static PaymentResponse FromEntity(PaymentTransaction transaction, string? explorerUrl) =>
        new(
            transaction.Id,
            transaction.OperationType.ToString().ToLowerInvariant(),
            transaction.State.ToString().ToLowerInvariant(),
            transaction.Amount.Satoshis,
            transaction.Fee?.Satoshis,
            transaction.BitcoinTxId?.Value,
            transaction.ConfirmationCount,
            explorerUrl,
            transaction.CreatedAt);
}
