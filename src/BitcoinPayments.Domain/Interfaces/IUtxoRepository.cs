using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Domain.Interfaces;

/// <summary>
/// Persists wallet UTXO tracking data.
/// </summary>
public interface IUtxoRepository
{
    /// <summary>
    /// Adds or updates a UTXO.
    /// </summary>
    Task AddOrUpdateAsync(Utxo utxo, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a batch of UTXOs.
    /// </summary>
    Task AddRangeAsync(IEnumerable<Utxo> utxos, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a UTXO by identifier.
    /// </summary>
    Task<Utxo?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all tracked UTXOs for a wallet.
    /// </summary>
    Task<IReadOnlyCollection<Utxo>> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists unspent UTXOs for a wallet.
    /// </summary>
    Task<IReadOnlyCollection<Utxo>> GetUnspentByWalletIdAsync(Guid walletId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a UTXO.
    /// </summary>
    Task UpdateAsync(Utxo utxo, CancellationToken cancellationToken);
}
