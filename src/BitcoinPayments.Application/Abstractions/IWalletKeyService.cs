using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.ValueObjects;
using NBitcoin;
using DomainBitcoinAddress = BitcoinPayments.Domain.ValueObjects.BitcoinAddress;

namespace BitcoinPayments.Application.Abstractions;

/// <summary>
/// Provides HD wallet creation and key derivation services.
/// </summary>
public interface IWalletKeyService
{
    /// <summary>
    /// Creates a new wallet aggregate with protected key material.
    /// </summary>
    Wallet CreateWallet(string name, string network);

    /// <summary>
    /// Derives an address for a wallet without mutating repository state.
    /// </summary>
    DerivedAddress DeriveAddress(Wallet wallet, WalletAddressPurpose purpose, int index);

    /// <summary>
    /// Derives the private key for a wallet path.
    /// </summary>
    Key GetPrivateKey(Wallet wallet, WalletAddressPurpose purpose, int index);
}

/// <summary>
/// Represents a derived wallet address and key metadata.
/// </summary>
/// <param name="Address">The derived address.</param>
/// <param name="Index">The derivation index.</param>
/// <param name="Purpose">The address purpose.</param>
/// <param name="PublicKeyHex">The compressed public key hex.</param>
public sealed record DerivedAddress(
    DomainBitcoinAddress Address,
    int Index,
    WalletAddressPurpose Purpose,
    string PublicKeyHex);
