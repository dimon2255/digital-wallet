using BitcoinPayments.Application.DTOs;
using MediatR;

namespace BitcoinPayments.Application.Commands;

/// <summary>
/// Captures a previous authorization.
/// </summary>
public sealed record CaptureAuthCommand(
    Guid AuthorizationId,
    long AmountSatoshis,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata,
    string IdempotencyKey) : IRequest<PaymentResponse>;
