namespace BitcoinPayments.Application.Abstractions;

/// <summary>
/// Exposes runtime Bitcoin configuration to the application layer.
/// </summary>
public interface IBitcoinSettings
{
    /// <summary>
    /// Gets the configured network name.
    /// </summary>
    string Network { get; }

    /// <summary>
    /// Gets the default fee rate in sat/vbyte.
    /// </summary>
    int DefaultFeeRateSatPerByte { get; }

    /// <summary>
    /// Gets the default authorization window in blocks.
    /// </summary>
    int AuthWindowBlocks { get; }

    /// <summary>
    /// Gets the confirmation threshold for settlement.
    /// </summary>
    int MinConfirmationsForSettlement { get; }

    /// <summary>
    /// Builds an explorer URL for a transaction.
    /// </summary>
    /// <param name="transactionId">The transaction id string.</param>
    /// <returns>The explorer URL, when available.</returns>
    string? BuildExplorerUrl(string? transactionId);
}
