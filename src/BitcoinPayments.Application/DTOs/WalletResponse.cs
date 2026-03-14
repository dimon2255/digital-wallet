namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a wallet summary response.
/// </summary>
/// <param name="Id">The wallet identifier.</param>
/// <param name="Name">The wallet name.</param>
/// <param name="Network">The network name.</param>
/// <param name="BalanceSatoshis">The tracked balance.</param>
/// <param name="CurrentReceivingIndex">The next receiving index.</param>
/// <param name="CurrentChangeIndex">The next change index.</param>
/// <param name="CreatedAt">The creation timestamp.</param>
public sealed record WalletResponse(
    Guid Id,
    string Name,
    string Network,
    long BalanceSatoshis,
    int CurrentReceivingIndex,
    int CurrentChangeIndex,
    DateTimeOffset CreatedAt);
