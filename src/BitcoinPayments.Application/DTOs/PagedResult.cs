namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a paginated result set.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
