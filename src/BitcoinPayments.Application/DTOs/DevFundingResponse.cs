namespace BitcoinPayments.Application.DTOs;

/// <summary>
/// Represents a development funding response.
/// </summary>
/// <param name="WalletId">The wallet identifier.</param>
/// <param name="Address">The address funded or to be funded.</param>
/// <param name="TransactionId">The resulting transaction id.</param>
/// <param name="Message">An operator-facing message.</param>
public sealed record DevFundingResponse(Guid WalletId, string Address, string? TransactionId, string Message);
