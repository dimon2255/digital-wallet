namespace BitcoinPayments.Infrastructure.Configuration;

/// <summary>
/// Defines runtime configuration for Bitcoin operations.
/// </summary>
public sealed class BitcoinOptions
{
    /// <summary>
    /// Gets or sets the Bitcoin network name.
    /// </summary>
    public string Network { get; set; } = "testnet4";

    /// <summary>
    /// Gets or sets the default fee rate in sat/vbyte.
    /// </summary>
    public int DefaultFeeRateSatPerByte { get; set; } = 2;

    /// <summary>
    /// Gets or sets the default authorization window in blocks.
    /// </summary>
    public int AuthWindowBlocks { get; set; } = 144;

    /// <summary>
    /// Gets or sets the confirmation threshold for settlement.
    /// </summary>
    public int MinConfirmationsForSettlement { get; set; } = 3;

    /// <summary>
    /// Gets or sets the max confirmations to track.
    /// </summary>
    public int MaxConfirmationsToTrack { get; set; } = 6;

    /// <summary>
    /// Gets or sets the public mempool API base URL.
    /// </summary>
    public string MempoolApiBaseUrl { get; set; } = "https://mempool.space/testnet4/api";

    /// <summary>
    /// Gets or sets a value indicating whether mainnet is allowed.
    /// </summary>
    public bool AllowMainnet { get; set; }

    /// <summary>
    /// Gets or sets the regtest RPC URI.
    /// </summary>
    public string? RpcUri { get; set; }

    /// <summary>
    /// Gets or sets the regtest RPC user.
    /// </summary>
    public string? RpcUser { get; set; }

    /// <summary>
    /// Gets or sets the regtest RPC password.
    /// </summary>
    public string? RpcPassword { get; set; }

    /// <summary>
    /// Gets or sets the transfer mode (auto, onchain, internal).
    /// </summary>
    public string TransferMode { get; set; } = "auto";
}
