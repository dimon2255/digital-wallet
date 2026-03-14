using BitcoinPayments.Domain.Exceptions;
using NBitcoin;

namespace BitcoinPayments.Domain.ValueObjects;

/// <summary>
/// Represents a Bitcoin transaction identifier.
/// </summary>
public readonly record struct TransactionId
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionId"/> struct.
    /// </summary>
    /// <param name="value">The transaction identifier.</param>
    public TransactionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationStateException("Transaction id cannot be empty.");
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Gets the raw transaction identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses a transaction identifier string.
    /// </summary>
    /// <param name="value">The transaction identifier.</param>
    /// <returns>A value object.</returns>
    public static TransactionId Parse(string value) => new(value);

    /// <summary>
    /// Converts the value to a uint256.
    /// </summary>
    /// <returns>The parsed hash.</returns>
    public uint256 ToUInt256() => uint256.Parse(Value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
