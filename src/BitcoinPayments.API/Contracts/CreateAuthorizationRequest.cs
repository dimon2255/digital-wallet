namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Represents an API request to create an authorization.
/// </summary>
public sealed record CreateAuthorizationRequest(
    Guid BuyerWalletId,
    Guid MerchantWalletId,
    long AmountSatoshis,
    int? AuthWindowBlocks,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata);
