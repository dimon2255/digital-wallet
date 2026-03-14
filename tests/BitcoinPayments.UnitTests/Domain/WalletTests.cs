using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;

namespace BitcoinPayments.UnitTests.Domain;

public sealed class WalletTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var wallet = new Wallet("test-wallet", "encrypted-key-data", "regtest");

        Assert.NotEqual(Guid.Empty, wallet.Id);
        Assert.Equal("test-wallet", wallet.Name);
        Assert.Equal("encrypted-key-data", wallet.EncryptedMasterKey);
        Assert.Equal("regtest", wallet.Network);
    }

    [Fact]
    public void ReserveNextIndex_Receiving_IncrementsReceivingIndex()
    {
        var wallet = new Wallet("w", "key", "regtest");

        var first = wallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var second = wallet.ReserveNextIndex(WalletAddressPurpose.Receiving);

        Assert.Equal(0, first);
        Assert.Equal(1, second);
    }

    [Fact]
    public void ReserveNextIndex_Change_IncrementsChangeIndex()
    {
        var wallet = new Wallet("w", "key", "regtest");

        var first = wallet.ReserveNextIndex(WalletAddressPurpose.Change);
        var second = wallet.ReserveNextIndex(WalletAddressPurpose.Change);

        Assert.Equal(0, first);
        Assert.Equal(1, second);
    }

    [Fact]
    public void ReserveNextIndex_IndependentCounters()
    {
        var wallet = new Wallet("w", "key", "regtest");

        var receiving0 = wallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var change0 = wallet.ReserveNextIndex(WalletAddressPurpose.Change);
        var receiving1 = wallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var change1 = wallet.ReserveNextIndex(WalletAddressPurpose.Change);

        Assert.Equal(0, receiving0);
        Assert.Equal(0, change0);
        Assert.Equal(1, receiving1);
        Assert.Equal(1, change1);
    }
}
