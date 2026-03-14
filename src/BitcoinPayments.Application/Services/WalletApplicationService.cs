using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.DTOs;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Application.Services;

/// <summary>
/// Implements wallet-focused use cases.
/// </summary>
public sealed class WalletApplicationService : IWalletApplicationService
{
    private readonly IWalletRepository walletRepository;
    private readonly IUtxoRepository utxoRepository;
    private readonly IWalletKeyService walletKeyService;
    private readonly WalletSynchronizationService walletSynchronizationService;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalletApplicationService"/> class.
    /// </summary>
    public WalletApplicationService(
        IWalletRepository walletRepository,
        IUtxoRepository utxoRepository,
        IWalletKeyService walletKeyService,
        WalletSynchronizationService walletSynchronizationService,
        IBitcoinNetwork bitcoinNetwork,
        IBitcoinSettings settings)
    {
        this.walletRepository = walletRepository;
        this.utxoRepository = utxoRepository;
        this.walletKeyService = walletKeyService;
        this.walletSynchronizationService = walletSynchronizationService;
        this.bitcoinNetwork = bitcoinNetwork;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<WalletResponse> CreateWalletAsync(string name, CancellationToken cancellationToken)
    {
        var wallet = walletKeyService.CreateWallet(name, settings.Network);
        await walletRepository.AddAsync(wallet, cancellationToken);
        return new WalletResponse(wallet.Id, wallet.Name, wallet.Network, 0L, wallet.CurrentReceivingIndex, wallet.CurrentChangeIndex, wallet.CreatedAt);
    }

    /// <inheritdoc />
    public async Task<WalletResponse> GetWalletAsync(Guid walletId, CancellationToken cancellationToken)
    {
        var wallet = await GetRequiredWalletAsync(walletId, cancellationToken);
        await walletSynchronizationService.SynchronizeAsync(wallet, cancellationToken);
        var utxos = await utxoRepository.GetUnspentByWalletIdAsync(walletId, cancellationToken);
        return new WalletResponse(
            wallet.Id,
            wallet.Name,
            wallet.Network,
            utxos.Sum(utxo => utxo.Amount.Satoshis),
            wallet.CurrentReceivingIndex,
            wallet.CurrentChangeIndex,
            wallet.CreatedAt);
    }

    /// <inheritdoc />
    public async Task<WalletAddressResponse> GetNextReceivingAddressAsync(Guid walletId, CancellationToken cancellationToken)
    {
        var wallet = await GetRequiredWalletAsync(walletId, cancellationToken);
        var index = wallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var derivedAddress = walletKeyService.DeriveAddress(wallet, WalletAddressPurpose.Receiving, index);
        await walletRepository.UpdateAsync(wallet, cancellationToken);
        return new WalletAddressResponse(wallet.Id, derivedAddress.Address.Value, derivedAddress.Index);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<UtxoResponse>> GetUtxosAsync(Guid walletId, CancellationToken cancellationToken)
    {
        var wallet = await GetRequiredWalletAsync(walletId, cancellationToken);
        await walletSynchronizationService.SynchronizeAsync(wallet, cancellationToken);
        var utxos = await utxoRepository.GetByWalletIdAsync(walletId, cancellationToken);
        return utxos
            .OrderByDescending(utxo => utxo.ConfirmationCount)
            .ThenBy(utxo => utxo.TransactionId.Value)
            .Select(utxo => new UtxoResponse(
                utxo.Id,
                utxo.TransactionId.Value,
                utxo.OutputIndex,
                utxo.Amount.Satoshis,
                utxo.Address.Value,
                utxo.ConfirmationCount,
                utxo.IsSpent,
                utxo.IsEscrow))
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<DevFundingResponse> FundWalletAsync(Guid walletId, long amountSatoshis, CancellationToken cancellationToken)
    {
        var wallet = await GetRequiredWalletAsync(walletId, cancellationToken);
        var index = wallet.ReserveNextIndex(WalletAddressPurpose.Receiving);
        var derivedAddress = walletKeyService.DeriveAddress(wallet, WalletAddressPurpose.Receiving, index);
        await walletRepository.UpdateAsync(wallet, cancellationToken);

        var fundingResult = await bitcoinNetwork.FundAddressAsync(
            derivedAddress.Address,
            Money.FromSatoshis(amountSatoshis),
            cancellationToken);

        await walletSynchronizationService.SynchronizeAsync(wallet, cancellationToken);

        return new DevFundingResponse(
            wallet.Id,
            derivedAddress.Address.Value,
            fundingResult.TransactionId?.Value,
            fundingResult.Message);
    }

    private async Task<Wallet> GetRequiredWalletAsync(Guid walletId, CancellationToken cancellationToken) =>
        await walletRepository.GetByIdAsync(walletId, cancellationToken)
        ?? throw new InvalidOperationStateException($"Wallet '{walletId}' was not found.");
}
