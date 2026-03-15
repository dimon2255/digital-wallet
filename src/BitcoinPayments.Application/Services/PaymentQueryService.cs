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
    private readonly IWalletRepository walletRepository;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentQueryService"/> class.
    /// </summary>
    public PaymentQueryService(
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        IBitcoinSettings settings)
    {
        this.transactionRepository = transactionRepository;
        this.walletRepository = walletRepository;
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
                transaction.ErrorMessage,
                transaction.BuyerWalletId,
                transaction.MerchantWalletId);
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
                transaction.ErrorMessage,
                transaction.BuyerWalletId,
                transaction.MerchantWalletId))
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<PagedResult<TransactionStatusResponse>> ListByUserAsync(
        string userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var wallets = await walletRepository.ListByUserIdAsync(userId, cancellationToken);
        var walletIds = wallets.Select(w => w.Id).ToArray();

        if (walletIds.Length == 0)
        {
            return new PagedResult<TransactionStatusResponse>([], 0, page, pageSize);
        }

        var (items, totalCount) = await transactionRepository.ListByWalletIdsAsync(walletIds, page, pageSize, cancellationToken);

        var responses = items
            .Select(t => new TransactionStatusResponse(
                t.Id,
                t.OperationType.ToString().ToLowerInvariant(),
                t.State.ToString().ToLowerInvariant(),
                t.Amount.Satoshis,
                t.Fee?.Satoshis,
                t.BitcoinTxId?.Value,
                t.ConfirmationCount,
                settings.BuildExplorerUrl(t.BitcoinTxId?.Value),
                t.CreatedAt,
                t.UpdatedAt,
                t.ErrorMessage,
                t.BuyerWalletId,
                t.MerchantWalletId))
            .ToArray();

        return new PagedResult<TransactionStatusResponse>(responses, totalCount, page, pageSize);
    }
}
