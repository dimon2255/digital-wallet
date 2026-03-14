using BitcoinPayments.Application.DTOs;
using MediatR;

namespace BitcoinPayments.Application.Commands;

/// <summary>
/// Creates an authorization escrow.
/// </summary>
public sealed record CreateAuthCommand(
    Guid BuyerWalletId,
    Guid MerchantWalletId,
    long AmountSatoshis,
    int? AuthWindowBlocks,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata,
    string IdempotencyKey) : IRequest<AuthorizationResponse>;
