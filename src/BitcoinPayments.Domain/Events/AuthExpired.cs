namespace BitcoinPayments.Domain.Events;

/// <summary>
/// Raised when an authorization escrow expires.
/// </summary>
/// <param name="PaymentId">The authorization payment identifier.</param>
/// <param name="EscrowId">The escrow identifier.</param>
public sealed record AuthExpired(Guid PaymentId, Guid EscrowId);
