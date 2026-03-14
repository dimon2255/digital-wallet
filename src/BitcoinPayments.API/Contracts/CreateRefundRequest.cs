namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Represents an API request to create a refund.
/// </summary>
public sealed record CreateRefundRequest(
    Guid ParentTransactionId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata);
