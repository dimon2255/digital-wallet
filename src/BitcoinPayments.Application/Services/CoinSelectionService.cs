using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Application.Services;

/// <summary>
/// Selects wallet UTXOs for outgoing transactions.
/// </summary>
public sealed class CoinSelectionService
{
    private const int EstimatedSegwitInputVBytes = 68;
    private const int EstimatedSegwitOutputVBytes = 31;
    private const int TransactionOverheadVBytes = 10;

    /// <summary>
    /// Selects UTXOs using a largest-first strategy.
    /// </summary>
    /// <param name="availableUtxos">Available UTXOs.</param>
    /// <param name="targetAmount">The amount to send.</param>
    /// <param name="feeRateSatPerByte">The fee rate in sat/vbyte.</param>
    /// <param name="outputCount">The expected number of outputs.</param>
    /// <returns>The selected UTXOs.</returns>
    public IReadOnlyCollection<Utxo> SelectLargestFirst(
        IReadOnlyCollection<Utxo> availableUtxos,
        Money targetAmount,
        int feeRateSatPerByte,
        int outputCount)
    {
        var selected = new List<Utxo>();
        var total = Money.Zero;

        foreach (var utxo in availableUtxos
                     .Where(candidate => !candidate.IsSpent)
                     .OrderByDescending(candidate => candidate.Amount.Satoshis))
        {
            selected.Add(utxo);
            total += utxo.Amount;

            var estimatedFee = EstimateFee(selected.Count, outputCount, feeRateSatPerByte);
            if (total >= targetAmount + estimatedFee)
            {
                return selected;
            }
        }

        throw new InsufficientFundsException(
            $"Available balance is {total.Satoshis} sat but {targetAmount.Satoshis} sat plus fees is required.");
    }

    /// <summary>
    /// Estimates a fee for a transaction skeleton.
    /// </summary>
    /// <param name="inputCount">The input count.</param>
    /// <param name="outputCount">The output count.</param>
    /// <param name="feeRateSatPerByte">The fee rate in sat/vbyte.</param>
    /// <returns>The estimated fee.</returns>
    public Money EstimateFee(int inputCount, int outputCount, int feeRateSatPerByte)
    {
        var estimatedSize = TransactionOverheadVBytes +
                            (inputCount * EstimatedSegwitInputVBytes) +
                            (outputCount * EstimatedSegwitOutputVBytes);

        return Money.FromSatoshis(estimatedSize * feeRateSatPerByte);
    }
}
