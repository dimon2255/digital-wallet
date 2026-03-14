namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Represents an API request to capture an authorization.
/// </summary>
public sealed record CaptureAuthorizationRequest(
    Guid AuthorizationId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata);
