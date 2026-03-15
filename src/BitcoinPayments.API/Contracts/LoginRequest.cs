namespace BitcoinPayments.API.Contracts;

/// <summary>
/// Login request contract.
/// </summary>
public sealed record LoginRequest(string Email, string Password);
