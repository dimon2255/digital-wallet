using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Application.Abstractions;

/// <summary>
/// Context for a transfer operation.
/// </summary>
public sealed record TransferContext(
    Wallet SourceWallet,
    Wallet DestinationWallet,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    string? IdempotencyKey);

/// <summary>
/// Strategy for executing transfers.
/// </summary>
public interface ITransferStrategy
{
    /// <summary>
    /// Executes the transfer and returns the resulting transaction.
    /// </summary>
    Task<PaymentTransaction> ExecuteAsync(TransferContext context, CancellationToken cancellationToken);
}
