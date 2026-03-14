using BitcoinPayments.Application.Services;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.UnitTests.Application;

public sealed class CoinSelectionServiceTests
{
    [Fact]
    public void SelectLargestFirst_ShouldPickLargestUtxosUntilTargetAndFeesAreCovered()
    {
        var service = new CoinSelectionService();
        var utxos = new[]
        {
            CreateUtxo(4_000),
            CreateUtxo(15_000),
            CreateUtxo(7_500),
        };

        var selected = service.SelectLargestFirst(utxos, Money.FromSatoshis(16_000), 2, 2);

        Assert.Equal(2, selected.Count);
        var amounts = selected.Select(utxo => utxo.Amount.Satoshis).OrderByDescending(amount => amount).ToArray();
        Assert.Equal(new long[] { 15_000, 7_500 }, amounts);
    }

    [Fact]
    public void SelectLargestFirst_ShouldThrowWhenFundsAreInsufficient()
    {
        var service = new CoinSelectionService();
        var utxos = new[]
        {
            CreateUtxo(1_000),
            CreateUtxo(2_000),
        };

        Assert.Throws<BitcoinPayments.Domain.Exceptions.InsufficientFundsException>(
            () => service.SelectLargestFirst(utxos, Money.FromSatoshis(10_000), 5, 2));
    }

    private static Utxo CreateUtxo(long amount) =>
        new()
        {
            WalletId = Guid.NewGuid(),
            TransactionId = TransactionId.Parse(Guid.NewGuid().ToString("N").PadLeft(64, '0').Substring(0, 64)),
            OutputIndex = 0,
            Amount = Money.FromSatoshis(amount),
            ScriptPubKey = "0014abcdefabcdefabcdefabcdefabcdefabcdef",
            Address = BitcoinAddress.Parse("tb1qexampleaddress0000000000000000000000"),
            DerivationIndex = 0,
        };
}
