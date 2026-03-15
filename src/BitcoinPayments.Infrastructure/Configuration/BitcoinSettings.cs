using BitcoinPayments.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace BitcoinPayments.Infrastructure.Configuration;

/// <summary>
/// Adapts runtime Bitcoin options to the application abstraction.
/// </summary>
public sealed class BitcoinSettings : IBitcoinSettings
{
    private readonly BitcoinOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitcoinSettings"/> class.
    /// </summary>
    public BitcoinSettings(IOptions<BitcoinOptions> options)
    {
        this.options = options.Value;
    }

    /// <inheritdoc />
    public string Network => options.Network;

    /// <inheritdoc />
    public int DefaultFeeRateSatPerByte => options.DefaultFeeRateSatPerByte;

    /// <inheritdoc />
    public int AuthWindowBlocks => options.AuthWindowBlocks;

    /// <inheritdoc />
    public int MinConfirmationsForSettlement => options.MinConfirmationsForSettlement;

    /// <inheritdoc />
    public string TransferMode => options.TransferMode;

    /// <inheritdoc />
    public string? BuildExplorerUrl(string? transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return null;
        }

        return options.Network.Trim().ToLowerInvariant() switch
        {
            "testnet4" => $"https://mempool.space/testnet4/tx/{transactionId}",
            "regtest" => null,
            _ => null,
        };
    }
}
