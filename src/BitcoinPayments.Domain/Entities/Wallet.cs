using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;

namespace BitcoinPayments.Domain.Entities;

/// <summary>
/// Represents an HD wallet tracked by the platform.
/// </summary>
public class Wallet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Wallet"/> class.
    /// </summary>
    public Wallet()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Wallet"/> class.
    /// </summary>
    /// <param name="name">Wallet display name.</param>
    /// <param name="encryptedMasterKey">Protected extended private key.</param>
    /// <param name="network">Network name.</param>
    public Wallet(string name, string encryptedMasterKey, string network)
    {
        Id = Guid.NewGuid();
        Name = name;
        EncryptedMasterKey = encryptedMasterKey;
        Network = network;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets or sets the wallet identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the wallet name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the protected BIP32 extended private key.
    /// </summary>
    public string EncryptedMasterKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the next receiving index.
    /// </summary>
    public int CurrentReceivingIndex { get; set; }

    /// <summary>
    /// Gets or sets the next change index.
    /// </summary>
    public int CurrentChangeIndex { get; set; }

    /// <summary>
    /// Gets or sets the configured network name.
    /// </summary>
    public string Network { get; set; } = "testnet4";

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the update timestamp.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Reserves the next derivation index for the requested chain.
    /// </summary>
    /// <param name="purpose">The address purpose.</param>
    /// <returns>The reserved index.</returns>
    public int ReserveNextIndex(WalletAddressPurpose purpose)
    {
        var index = purpose switch
        {
            WalletAddressPurpose.Receiving => CurrentReceivingIndex++,
            WalletAddressPurpose.Change => CurrentChangeIndex++,
            _ => throw new InvalidOperationStateException($"Unknown wallet address purpose '{purpose}'."),
        };

        UpdatedAt = DateTimeOffset.UtcNow;
        return index;
    }
}
