using BitcoinPayments.Application.DTOs;
using MediatR;

namespace BitcoinPayments.Application.Commands;

/// <summary>
/// Creates a refund transaction.
/// </summary>
public sealed record CreateRefundCommand(
    Guid ParentTransactionId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata,
    string IdempotencyKey) : IRequest<PaymentResponse>;
