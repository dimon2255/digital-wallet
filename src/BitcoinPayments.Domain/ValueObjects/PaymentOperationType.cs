namespace BitcoinPayments.Domain.ValueObjects;

/// <summary>
/// Defines the supported payment operation types.
/// </summary>
public enum PaymentOperationType
{
    /// <summary>
    /// Direct charge or sale.
    /// </summary>
    Charge = 1,

    /// <summary>
    /// Authorization hold using escrow.
    /// </summary>
    Auth = 2,

    /// <summary>
    /// Capture of an existing authorization.
    /// </summary>
    Capture = 3,

    /// <summary>
    /// Cooperative or automatic void of an authorization.
    /// </summary>
    Void = 4,

    /// <summary>
    /// Refund after settlement.
    /// </summary>
    Refund = 5,
}
