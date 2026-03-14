namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a basic payment initiation request.
/// </summary>
/// <param name="BuyerWalletId">The buyer wallet identifier.</param>
/// <param name="MerchantWalletId">The merchant wallet identifier.</param>
/// <param name="AmountSatoshis">The requested amount in satoshis.</param>
/// <param name="FeeRateSatPerByte">The optional fee rate override.</param>
/// <param name="Metadata">Optional metadata.</param>
public sealed record PaymentRequest(
    Guid BuyerWalletId,
    Guid MerchantWalletId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata);
