namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Authentication response contract.
/// </summary>
public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    UserInfo User);

/// <summary>
/// User information included in auth responses.
/// </summary>
public sealed record UserInfo(string Id, string Email, string? DisplayName);
