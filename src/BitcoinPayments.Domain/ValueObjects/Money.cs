using NBitcoin;

namespace BitcoinPayments.Domain.ValueObjects;

/// <summary>
/// Represents a Bitcoin amount in satoshis.
/// </summary>
public readonly record struct Money(long Satoshis) : IComparable<Money>
{
    /// <summary>
    /// Gets a zero-value amount.
    /// </summary>
    public static Money Zero => new(0L);

    /// <summary>
    /// Creates a value object from a satoshi amount.
    /// </summary>
    /// <param name="satoshis">The satoshi amount.</param>
    /// <returns>A money value.</returns>
    public static Money FromSatoshis(long satoshis) => new(satoshis);

    /// <summary>
    /// Converts the value object to the NBitcoin representation.
    /// </summary>
    /// <returns>An <see cref="NBitcoin.Money"/> value.</returns>
    public NBitcoin.Money ToNBitcoin() => NBitcoin.Money.Satoshis(Satoshis);

    /// <summary>
    /// Compares the current value with another amount.
    /// </summary>
    /// <param name="other">The other value.</param>
    /// <returns>A comparison result.</returns>
    public int CompareTo(Money other) => Satoshis.CompareTo(other.Satoshis);

    /// <summary>
    /// Adds two amounts.
    /// </summary>
    public static Money operator +(Money left, Money right) => new(left.Satoshis + right.Satoshis);

    /// <summary>
    /// Subtracts two amounts.
    /// </summary>
    public static Money operator -(Money left, Money right) => new(left.Satoshis - right.Satoshis);

    /// <summary>
    /// Multiplies the amount by a scalar.
    /// </summary>
    public static Money operator *(Money money, int factor) => new(money.Satoshis * factor);

    /// <summary>
    /// Determines whether one amount is greater than another.
    /// </summary>
    public static bool operator >(Money left, Money right) => left.Satoshis > right.Satoshis;

    /// <summary>
    /// Determines whether one amount is less than another.
    /// </summary>
    public static bool operator <(Money left, Money right) => left.Satoshis < right.Satoshis;

    /// <summary>
    /// Determines whether one amount is greater than or equal to another.
    /// </summary>
    public static bool operator >=(Money left, Money right) => left.Satoshis >= right.Satoshis;

    /// <summary>
    /// Determines whether one amount is less than or equal to another.
    /// </summary>
    public static bool operator <=(Money left, Money right) => left.Satoshis <= right.Satoshis;

    /// <inheritdoc />
    public override string ToString() => $"{Satoshis} sat";
}
