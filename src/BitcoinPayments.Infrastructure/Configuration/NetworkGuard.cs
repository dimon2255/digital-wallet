using BitcoinPayments.Domain.Exceptions;
using NBitcoin;

namespace BitcoinPayments.Infrastructure.Configuration;

/// <summary>
/// Prevents the application from running on mainnet.
/// </summary>
public static class NetworkGuard
{
    /// <summary>
    /// Validates the configured network.
    /// </summary>
    public static void EnsureTestnet(BitcoinOptions options)
    {
        var configuredNetwork = options.Network.Trim().ToLowerInvariant();
        if (configuredNetwork is "mainnet" or "main")
        {
            throw new MainnetGuardException(
                "FATAL: This application is configured for TESTNET ONLY. Mainnet operation is not permitted.");
        }

        var resolved = configuredNetwork switch
        {
            "testnet4" => Network.GetNetwork("testnet4") ?? Network.TestNet,
            "regtest" => Network.RegTest,
            _ => throw new MainnetGuardException($"FATAL: Unsupported network '{options.Network}'."),
        };

        if (resolved == Network.Main)
        {
            throw new MainnetGuardException("FATAL: Resolved network is mainnet. Aborting.");
        }

        if (options.AllowMainnet)
        {
            throw new MainnetGuardException("FATAL: AllowMainnet must remain false in this build.");
        }
    }
}
