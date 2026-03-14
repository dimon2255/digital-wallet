using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.ValueObjects;
using Microsoft.AspNetCore.DataProtection;
using NBitcoin;
using DomainBitcoinAddress = BitcoinPayments.Domain.ValueObjects.BitcoinAddress;

namespace BitcoinPayments.Infrastructure.Bitcoin;

/// <summary>
/// Implements HD wallet generation and derivation using NBitcoin.
/// </summary>
public sealed class HdWalletManager : IWalletKeyService
{
    private static readonly KeyPath AccountKeyPath = new("m/44'/1'/0'");
    private readonly IDataProtector dataProtector;

    /// <summary>
    /// Initializes a new instance of the <see cref="HdWalletManager"/> class.
    /// </summary>
    public HdWalletManager(IDataProtectionProvider dataProtectionProvider)
    {
        dataProtector = dataProtectionProvider.CreateProtector("BitcoinPayments.Infrastructure.Bitcoin.HdWalletManager");
    }

    /// <inheritdoc />
    public Wallet CreateWallet(string name, string network)
    {
        var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
        var protectedMnemonic = dataProtector.Protect(mnemonic.ToString());
        return new Wallet(name, protectedMnemonic, network);
    }

    /// <inheritdoc />
    public DerivedAddress DeriveAddress(Wallet wallet, WalletAddressPurpose purpose, int index)
    {
        var nbitcoinNetwork = ResolveNetwork(wallet.Network);
        var privateKey = GetPrivateKey(wallet, purpose, index);
        var address = privateKey.PubKey.GetAddress(ScriptPubKeyType.Segwit, nbitcoinNetwork);
        return new DerivedAddress(
            DomainBitcoinAddress.Parse(address.ToString()),
            index,
            purpose,
            privateKey.PubKey.ToHex());
    }

    /// <inheritdoc />
    public Key GetPrivateKey(Wallet wallet, WalletAddressPurpose purpose, int index)
    {
        var mnemonicText = dataProtector.Unprotect(wallet.EncryptedMasterKey);
        var accountKey = new Mnemonic(mnemonicText).DeriveExtKey().Derive(AccountKeyPath);
        var chain = purpose == WalletAddressPurpose.Receiving ? 0 : 1;
        var derived = accountKey.Derive((uint)chain).Derive((uint)index);
        return derived.PrivateKey;
    }

    private static Network ResolveNetwork(string networkName) =>
        networkName.Trim().ToLowerInvariant() switch
        {
            "testnet4" => Network.GetNetwork("testnet4") ?? Network.TestNet,
            "regtest" => Network.RegTest,
            _ => throw new MainnetGuardException($"Unsupported or unsafe network '{networkName}'."),
        };
}
