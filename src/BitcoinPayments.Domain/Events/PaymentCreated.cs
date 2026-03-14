using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Domain.Events;

/// <summary>
/// Raised when a new payment transaction is created.
/// </summary>
/// <param name="PaymentId">The payment identifier.</param>
/// <param name="OperationType">The operation type.</param>
/// <param name="Amount">The requested amount.</param>
public sealed record PaymentCreated(Guid PaymentId, PaymentOperationType OperationType, Money Amount);
