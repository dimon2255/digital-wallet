namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a newly derived wallet address.
/// </summary>
/// <param name="WalletId">The wallet identifier.</param>
/// <param name="Address">The address string.</param>
/// <param name="Index">The derivation index.</param>
public sealed record WalletAddressResponse(Guid WalletId, string Address, int Index);
