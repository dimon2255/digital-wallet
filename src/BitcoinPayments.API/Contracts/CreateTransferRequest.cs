namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Transfer request contract.
/// </summary>
public sealed record CreateTransferRequest(
    Guid SourceWalletId,
    Guid DestinationWalletId,
    long AmountSatoshis,
    int? FeeRateSatPerByte);
