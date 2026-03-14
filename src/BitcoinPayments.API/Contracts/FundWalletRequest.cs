namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Represents a dev-only wallet funding request.
/// </summary>
public sealed record FundWalletRequest(Guid WalletId, long AmountSatoshis);
