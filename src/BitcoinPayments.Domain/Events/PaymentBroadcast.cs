using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Domain.Events;

/// <summary>
/// Raised when a payment transaction is broadcast.
/// </summary>
/// <param name="PaymentId">The payment identifier.</param>
/// <param name="BitcoinTransactionId">The on-chain transaction identifier.</param>
public sealed record PaymentBroadcast(Guid PaymentId, TransactionId BitcoinTransactionId);
