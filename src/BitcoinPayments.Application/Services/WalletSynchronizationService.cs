using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;

namespace BitcoinPayments.Application.Services;

/// <summary>
/// Synchronizes tracked wallet UTXOs from the configured Bitcoin network.
/// </summary>
public sealed class WalletSynchronizationService
{
    private readonly IWalletKeyService walletKeyService;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly IUtxoRepository utxoRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalletSynchronizationService"/> class.
    /// </summary>
    public WalletSynchronizationService(
        IWalletKeyService walletKeyService,
        IBitcoinNetwork bitcoinNetwork,
        IUtxoRepository utxoRepository)
    {
        this.walletKeyService = walletKeyService;
        this.bitcoinNetwork = bitcoinNetwork;
        this.utxoRepository = utxoRepository;
    }

    /// <summary>
    /// Synchronizes tracked addresses for a wallet.
    /// </summary>
    public async Task SynchronizeAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        var derivedAddresses = new List<DerivedAddress>();

        for (var index = 0; index < wallet.CurrentReceivingIndex; index++)
        {
            derivedAddresses.Add(walletKeyService.DeriveAddress(wallet, WalletAddressPurpose.Receiving, index));
        }

        for (var index = 0; index < wallet.CurrentChangeIndex; index++)
        {
            derivedAddresses.Add(walletKeyService.DeriveAddress(wallet, WalletAddressPurpose.Change, index));
        }

        if (derivedAddresses.Count == 0)
        {
            return;
        }

        var addressLookup = derivedAddresses.ToDictionary(entry => entry.Address.Value, StringComparer.OrdinalIgnoreCase);
        var observedUtxos = await bitcoinNetwork.GetAddressUtxosAsync(
            derivedAddresses.Select(entry => entry.Address).ToArray(),
            cancellationToken);

        foreach (var observed in observedUtxos)
        {
            var derived = addressLookup[observed.Address.Value];
            await utxoRepository.AddOrUpdateAsync(
                new Utxo
                {
                    WalletId = wallet.Id,
                    TransactionId = observed.TransactionId,
                    OutputIndex = observed.OutputIndex,
                    Amount = observed.Amount,
                    ScriptPubKey = observed.ScriptPubKey,
                    Address = observed.Address,
                    ConfirmationCount = observed.ConfirmationCount,
                    DerivationIndex = derived.Index,
                    IsChangeAddress = derived.Purpose == WalletAddressPurpose.Change,
                },
                cancellationToken);
        }
    }
}
