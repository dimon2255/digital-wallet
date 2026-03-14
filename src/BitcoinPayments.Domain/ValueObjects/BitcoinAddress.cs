using BitcoinPayments.Domain.Exceptions;
using NBitcoin;

namespace BitcoinPayments.Domain.ValueObjects;

/// <summary>
/// Represents a validated Bitcoin address string.
/// </summary>
public readonly record struct BitcoinAddress
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BitcoinAddress"/> struct.
    /// </summary>
    /// <param name="value">The address string.</param>
    public BitcoinAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationStateException("Bitcoin address cannot be empty.");
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the raw address string.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses an address string.
    /// </summary>
    /// <param name="value">The address string.</param>
    /// <returns>A value object.</returns>
    public static BitcoinAddress Parse(string value) => new(value);

    /// <summary>
    /// Converts the value to an NBitcoin address.
    /// </summary>
    /// <param name="network">The target network.</param>
    /// <returns>An NBitcoin address.</returns>
    public NBitcoin.BitcoinAddress ToNBitcoin(Network network) => NBitcoin.BitcoinAddress.Create(Value, network);

    /// <inheritdoc />
    public override string ToString() => Value;
}
