using BitcoinPayments.Application.DTOs;
using MediatR;

namespace BitcoinPayments.Application.Commands;

/// <summary>
/// Command to create a wallet-to-wallet transfer.
/// </summary>
public sealed record CreateTransferCommand(
    Guid SourceWalletId,
    Guid DestinationWalletId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    string? IdempotencyKey,
    string UserId) : IRequest<PaymentResponse>;
