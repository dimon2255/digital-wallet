using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.DTOs;
using BitcoinPayments.Domain.Interfaces;

namespace BitcoinPayments.Application.Services;

/// <summary>
/// Builds read models for payment endpoints.
/// </summary>
public sealed class PaymentQueryService : IPaymentQueryService
{
    private readonly ITransactionRepository transactionRepository;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentQueryService"/> class.
    /// </summary>
    public PaymentQueryService(ITransactionRepository transactionRepository, IBitcoinSettings settings)
    {
        this.transactionRepository = transactionRepository;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<TransactionStatusResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(id, cancellationToken);
        return transaction is null
            ? null
            : new TransactionStatusResponse(
                transaction.Id,
                transaction.OperationType.ToString().ToLowerInvariant(),
                transaction.State.ToString().ToLowerInvariant(),
                transaction.Amount.Satoshis,
                transaction.Fee?.Satoshis,
                transaction.BitcoinTxId?.Value,
                transaction.ConfirmationCount,
                settings.BuildExplorerUrl(transaction.BitcoinTxId?.Value),
                transaction.CreatedAt,
                transaction.UpdatedAt,
                transaction.ErrorMessage);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TransactionStatusResponse>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var root = await transactionRepository.GetByIdAsync(id, cancellationToken);
        if (root is null)
        {
            return Array.Empty<TransactionStatusResponse>();
        }

        var children = await transactionRepository.GetChildrenAsync(id, cancellationToken);
        return new[] { root }
            .Concat(children)
            .OrderBy(transaction => transaction.CreatedAt)
            .Select(transaction => new TransactionStatusResponse(
                transaction.Id,
                transaction.OperationType.ToString().ToLowerInvariant(),
                transaction.State.ToString().ToLowerInvariant(),
                transaction.Amount.Satoshis,
                transaction.Fee?.Satoshis,
                transaction.BitcoinTxId?.Value,
                transaction.ConfirmationCount,
                settings.BuildExplorerUrl(transaction.BitcoinTxId?.Value),
                transaction.CreatedAt,
                transaction.UpdatedAt,
                transaction.ErrorMessage))
            .ToArray();
    }
}
