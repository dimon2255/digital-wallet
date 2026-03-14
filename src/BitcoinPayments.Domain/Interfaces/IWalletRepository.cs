using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Domain.Interfaces;

/// <summary>
/// Persists wallet aggregates.
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    /// Adds a wallet.
    /// </summary>
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a wallet by identifier.
    /// </summary>
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a wallet by name.
    /// </summary>
    Task<Wallet?> GetByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all wallets.
    /// </summary>
    Task<IReadOnlyCollection<Wallet>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates a wallet.
    /// </summary>
    Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken);
}
