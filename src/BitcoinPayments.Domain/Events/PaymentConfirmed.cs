namespace BitcoinPayments.Domain.Events;

/// <summary>
/// Raised when a payment reaches a new confirmation threshold.
/// </summary>
/// <param name="PaymentId">The payment identifier.</param>
/// <param name="ConfirmationCount">The confirmation count.</param>
public sealed record PaymentConfirmed(Guid PaymentId, int ConfirmationCount);
