using BitcoinPayments.Application.DTOs;
using MediatR;

namespace BitcoinPayments.Application.Commands;

/// <summary>
/// Creates a direct charge transaction.
/// </summary>
public sealed record CreateChargeCommand(
    Guid BuyerWalletId,
    Guid MerchantWalletId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata,
    string IdempotencyKey) : IRequest<PaymentResponse>;
