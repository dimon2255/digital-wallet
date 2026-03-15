using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Domain.Interfaces;

/// <summary>
/// Persists payment transactions.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Adds a transaction.
    /// </summary>
    Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a transaction by identifier.
    /// </summary>
    Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a transaction by idempotency key.
    /// </summary>
    Task<PaymentTransaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);

    /// <summary>
    /// Lists child transactions for a parent operation.
    /// </summary>
    Task<IReadOnlyCollection<PaymentTransaction>> GetChildrenAsync(Guid parentTransactionId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists transactions that should be monitored for confirmations.
    /// </summary>
    Task<IReadOnlyCollection<PaymentTransaction>> ListPendingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Lists transactions for wallets owned by a user, with pagination.
    /// </summary>
    Task<(IReadOnlyCollection<PaymentTransaction> Items, int TotalCount)> ListByWalletIdsAsync(
        IReadOnlyCollection<Guid> walletIds,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates a transaction.
    /// </summary>
    Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken);
}
