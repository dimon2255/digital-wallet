namespace BitcoinPayments.Domain.Enums;

/// <summary>
/// Defines the lifecycle states of an escrow record.
/// </summary>
public enum EscrowState
{
    /// <summary>
    /// Escrow record created but not yet funded.
    /// </summary>
    Created = 1,

    /// <summary>
    /// Escrow funding output exists and is active.
    /// </summary>
    Funded = 2,

    /// <summary>
    /// Escrow has been captured to the merchant.
    /// </summary>
    Captured = 3,

    /// <summary>
    /// Escrow has been voided back to the buyer.
    /// </summary>
    Voided = 4,

    /// <summary>
    /// Escrow has expired.
    /// </summary>
    Expired = 5,
}
