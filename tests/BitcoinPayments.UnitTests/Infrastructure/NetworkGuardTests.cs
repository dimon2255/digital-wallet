using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Infrastructure.Configuration;

namespace BitcoinPayments.UnitTests.Infrastructure;

public sealed class NetworkGuardTests
{
    [Fact]
    public void EnsureTestnet_ShouldThrowForMainnet()
    {
        var options = new BitcoinOptions
        {
            Network = "mainnet",
            AllowMainnet = false,
        };

        Assert.Throws<MainnetGuardException>(() => NetworkGuard.EnsureTestnet(options));
    }

    [Fact]
    public void EnsureTestnet_ShouldAcceptRegtest()
    {
        var options = new BitcoinOptions
        {
            Network = "regtest",
            AllowMainnet = false,
        };

        var exception = Record.Exception(() => NetworkGuard.EnsureTestnet(options));
        Assert.Null(exception);
    }
}
