namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Represents an API request to create a direct charge.
/// </summary>
public sealed record CreateChargeRequest(
    Guid BuyerWalletId,
    Guid MerchantWalletId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata);
