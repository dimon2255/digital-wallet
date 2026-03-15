using BitcoinPayments.Application.DTOs;

namespace BitcoinPayments.Application.Abstractions;

/// <summary>
/// Exposes payment read models.
/// </summary>
public interface IPaymentQueryService
{
    /// <summary>
    /// Gets a payment by identifier.
    /// </summary>
    Task<TransactionStatusResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a payment and its related child operations.
    /// </summary>
    Task<IReadOnlyCollection<TransactionStatusResponse>> GetHistoryAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Lists transactions for a user with pagination.
    /// </summary>
    Task<PagedResult<TransactionStatusResponse>> ListByUserAsync(string userId, int page, int pageSize, CancellationToken cancellationToken);
}
