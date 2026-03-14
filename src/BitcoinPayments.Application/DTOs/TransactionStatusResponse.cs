namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a payment status view model.
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
/// <param name="UpdatedAt">The update timestamp.</param>
/// <param name="ErrorMessage">The failure message.</param>
public sealed record TransactionStatusResponse(
    Guid Id,
    string OperationType,
    string State,
    long AmountSatoshis,
    long? FeeSatoshis,
    string? BitcoinTxId,
    int ConfirmationCount,
    string? ExplorerUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ErrorMessage);
