using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Domain.Interfaces;

/// <summary>
/// Persists escrow records.
/// </summary>
public interface IEscrowRepository
{
    /// <summary>
    /// Adds an escrow record.
    /// </summary>
    Task AddAsync(Escrow escrow, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an escrow by identifier.
    /// </summary>
    Task<Escrow?> GetByIdAsync(Guid escrowId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an escrow by its parent payment transaction identifier.
    /// </summary>
    Task<Escrow?> GetByPaymentTransactionIdAsync(Guid paymentTransactionId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists active escrows.
    /// </summary>
    Task<IReadOnlyCollection<Escrow>> ListActiveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates an escrow.
    /// </summary>
    Task UpdateAsync(Escrow escrow, CancellationToken cancellationToken);
}
