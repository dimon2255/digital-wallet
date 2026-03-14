namespace BitcoinPayments.Domain.Enums;

/// <summary>
/// Defines the lifecycle states of a payment transaction.
/// </summary>
public enum TransactionState
{
    /// <summary>
    /// Initial state before any network activity.
    /// </summary>
    Created = 1,

    /// <summary>
    /// The transaction is being broadcast.
    /// </summary>
    Broadcasting = 2,

    /// <summary>
    /// The transaction is in the mempool.
    /// </summary>
    Mempool = 3,

    /// <summary>
    /// Authorization funding transaction was broadcast.
    /// </summary>
    FundingBroadcast = 4,

    /// <summary>
    /// Authorization funding transaction confirmed.
    /// </summary>
    FundingConfirmed = 5,

    /// <summary>
    /// Authorization is active and available for capture or void.
    /// </summary>
    AuthActive = 6,

    /// <summary>
    /// Capture transaction was broadcast.
    /// </summary>
    CaptureBroadcast = 7,

    /// <summary>
    /// Capture transaction is confirming.
    /// </summary>
    CaptureConfirming = 8,

    /// <summary>
    /// Void transaction was broadcast.
    /// </summary>
    VoidBroadcast = 9,

    /// <summary>
    /// Void transaction is confirming.
    /// </summary>
    VoidConfirming = 10,

    /// <summary>
    /// General confirmation tracking state.
    /// </summary>
    Confirming = 11,

    /// <summary>
    /// Final settled state.
    /// </summary>
    Settled = 12,

    /// <summary>
    /// Terminal void state.
    /// </summary>
    Voided = 13,

    /// <summary>
    /// Terminal expired state for an authorization.
    /// </summary>
    Expired = 14,

    /// <summary>
    /// Terminal failure state.
    /// </summary>
    Failed = 15,
}
