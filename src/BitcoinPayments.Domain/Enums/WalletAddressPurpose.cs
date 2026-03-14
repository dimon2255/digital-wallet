namespace BitcoinPayments.Domain.Enums;

/// <summary>
/// Identifies the BIP44 chain used for a derived address.
/// </summary>
public enum WalletAddressPurpose
{
    /// <summary>
    /// External receiving chain.
    /// </summary>
    Receiving = 0,

    /// <summary>
    /// Internal change chain.
    /// </summary>
    Change = 1,
}
