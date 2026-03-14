using BitcoinPayments.Application.DTOs;
using MediatR;

namespace BitcoinPayments.Application.Commands;

/// <summary>
/// Voids a previous authorization.
/// </summary>
public sealed record VoidAuthCommand(
    Guid AuthorizationId,
    int? FeeRateSatPerByte,
    Dictionary<string, string?>? Metadata,
    string IdempotencyKey) : IRequest<PaymentResponse>;
