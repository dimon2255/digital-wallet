namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Registration request contract.
/// </summary>
public sealed record RegisterRequest(string Email, string Password, string? DisplayName);
