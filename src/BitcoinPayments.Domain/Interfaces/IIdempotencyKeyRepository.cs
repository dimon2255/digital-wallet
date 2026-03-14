using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Domain.Interfaces;

/// <summary>
/// Persists idempotent responses.
/// </summary>
public interface IIdempotencyKeyRepository
{
    /// <summary>
    /// Gets a record by key.
    /// </summary>
    Task<IdempotencyKeyRecord?> GetAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Stores or replaces a record.
    /// </summary>
    Task UpsertAsync(IdempotencyKeyRecord record, CancellationToken cancellationToken);
}
