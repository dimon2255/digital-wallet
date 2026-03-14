using BitcoinPayments.Domain.ValueObjects;
using DomainMoney = BitcoinPayments.Domain.ValueObjects.Money;

namespace BitcoinPayments.UnitTests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void Zero_HasZeroSatoshis()
    {
        var zero = DomainMoney.Zero;

        Assert.Equal(0L, zero.Satoshis);
    }

    [Fact]
    public void FromSatoshis_StoresValue()
    {
        var money = DomainMoney.FromSatoshis(5_000);

        Assert.Equal(5_000L, money.Satoshis);
    }

    [Fact]
    public void Addition_ReturnsSumOfSatoshis()
    {
        var a = DomainMoney.FromSatoshis(3_000);
        var b = DomainMoney.FromSatoshis(2_000);

        var result = a + b;

        Assert.Equal(5_000L, result.Satoshis);
    }

    [Fact]
    public void Subtraction_ReturnsDifference()
    {
        var a = DomainMoney.FromSatoshis(5_000);
        var b = DomainMoney.FromSatoshis(2_000);

        var result = a - b;

        Assert.Equal(3_000L, result.Satoshis);
    }

    [Fact]
    public void Multiplication_ReturnsProduct()
    {
        var money = DomainMoney.FromSatoshis(1_000);

        var result = money * 3;

        Assert.Equal(3_000L, result.Satoshis);
    }

    [Fact]
    public void GreaterThan_ComparesCorrectly()
    {
        var larger = DomainMoney.FromSatoshis(5_000);
        var smaller = DomainMoney.FromSatoshis(1_000);

        Assert.True(larger > smaller);
        Assert.False(smaller > larger);
    }

    [Fact]
    public void LessThan_ComparesCorrectly()
    {
        var larger = DomainMoney.FromSatoshis(5_000);
        var smaller = DomainMoney.FromSatoshis(1_000);

        Assert.True(smaller < larger);
        Assert.False(larger < smaller);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = DomainMoney.FromSatoshis(1_000);
        var b = DomainMoney.FromSatoshis(1_000);

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void CompareTo_ReturnsCorrectOrdering()
    {
        var small = DomainMoney.FromSatoshis(100);
        var medium = DomainMoney.FromSatoshis(500);
        var large = DomainMoney.FromSatoshis(1_000);

        Assert.True(small.CompareTo(medium) < 0);
        Assert.True(large.CompareTo(medium) > 0);
        Assert.Equal(0, medium.CompareTo(DomainMoney.FromSatoshis(500)));
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var money = DomainMoney.FromSatoshis(42_000);

        Assert.Equal("42000 sat", money.ToString());
    }
}
