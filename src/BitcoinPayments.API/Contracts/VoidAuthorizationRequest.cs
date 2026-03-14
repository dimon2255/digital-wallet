namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Represents an API request to void an authorization.
/// </summary>
public sealed record VoidAuthorizationRequest(
    Guid AuthorizationId,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata);
