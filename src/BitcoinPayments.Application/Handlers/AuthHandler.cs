using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.DTOs;
using BitcoinPayments.Application.Services;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;
using MediatR;

namespace BitcoinPayments.Application.Handlers;

/// <summary>
/// Handles authorization requests.
/// </summary>
public sealed class AuthHandler : IRequestHandler<CreateAuthCommand, AuthorizationResponse>
{
    private readonly IWalletRepository walletRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IUtxoRepository utxoRepository;
    private readonly IEscrowRepository escrowRepository;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly WalletSynchronizationService walletSynchronizationService;
    private readonly EscrowService escrowService;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthHandler"/> class.
    /// </summary>
    public AuthHandler(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IUtxoRepository utxoRepository,
        IEscrowRepository escrowRepository,
        IBitcoinNetwork bitcoinNetwork,
        WalletSynchronizationService walletSynchronizationService,
        EscrowService escrowService,
        IBitcoinSettings settings)
    {
        this.walletRepository = walletRepository;
        this.transactionRepository = transactionRepository;
        this.utxoRepository = utxoRepository;
        this.escrowRepository = escrowRepository;
        this.bitcoinNetwork = bitcoinNetwork;
        this.walletSynchronizationService = walletSynchronizationService;
        this.escrowService = escrowService;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<AuthorizationResponse> Handle(CreateAuthCommand request, CancellationToken cancellationToken)
    {
        var buyerWallet = await GetRequiredWalletAsync(request.BuyerWalletId, cancellationToken);
        var merchantWallet = await GetRequiredWalletAsync(request.MerchantWalletId, cancellationToken);

        await walletSynchronizationService.SynchronizeAsync(buyerWallet, cancellationToken);
        var availableUtxos = await utxoRepository.GetUnspentByWalletIdAsync(buyerWallet.Id, cancellationToken);
        var currentBlockHeight = await bitcoinNetwork.GetCurrentBlockHeightAsync(cancellationToken);

        var payment = new PaymentTransaction(
            request.IdempotencyKey,
            PaymentOperationType.Auth,
            Money.FromSatoshis(request.AmountSatoshis),
            buyerWallet.Id,
            merchantWallet.Id)
        {
            Metadata = request.Metadata ?? new Dictionary<string, string?>(),
        };

        await transactionRepository.AddAsync(payment, cancellationToken);

        try
        {
            var authWindowBlocks = request.AuthWindowBlocks.GetValueOrDefault(settings.AuthWindowBlocks);
            var buildResult = escrowService.BuildAuthorization(
                buyerWallet,
                merchantWallet,
                availableUtxos,
                Money.FromSatoshis(request.AmountSatoshis),
                authWindowBlocks,
                request.FeeRateSatPerByte,
                currentBlockHeight);

            var broadcastResult = await bitcoinNetwork.BroadcastTransactionAsync(
                buildResult.FundingTransaction.RawTransactionHex,
                cancellationToken);

            payment.MarkBroadcast(
                broadcastResult.TransactionId,
                buildResult.FundingTransaction.RawTransactionHex,
                buildResult.FundingTransaction.Fee,
                TransactionState.FundingBroadcast);

            buildResult.Escrow.PaymentTransactionId = payment.Id;

            foreach (var utxo in buildResult.FundingTransaction.ConsumedUtxos)
            {
                utxo.MarkSpent(broadcastResult.TransactionId);
                await utxoRepository.UpdateAsync(utxo, cancellationToken);
            }

            foreach (var output in buildResult.FundingTransaction.Outputs)
            {
                var trackedUtxo = new Utxo
                {
                    WalletId = output.WalletId,
                    TransactionId = broadcastResult.TransactionId,
                    OutputIndex = output.OutputIndex,
                    Amount = output.Amount,
                    ScriptPubKey = output.ScriptPubKeyHex,
                    Address = output.Address,
                    ConfirmationCount = 0,
                    IsEscrow = output.IsEscrow,
                    DerivationIndex = output.DerivationIndex,
                    IsChangeAddress = output.IsChange,
                };

                await utxoRepository.AddOrUpdateAsync(trackedUtxo, cancellationToken);
                if (output.IsEscrow)
                {
                    buildResult.Escrow.FundingUtxoId = trackedUtxo.Id;
                }
            }

            await escrowRepository.AddAsync(buildResult.Escrow, cancellationToken);
            await walletRepository.UpdateAsync(buyerWallet, cancellationToken);
            await walletRepository.UpdateAsync(merchantWallet, cancellationToken);
            await transactionRepository.UpdateAsync(payment, cancellationToken);

            return new AuthorizationResponse(
                payment.Id,
                payment.OperationType.ToString().ToLowerInvariant(),
                payment.State.ToString().ToLowerInvariant(),
                payment.Amount.Satoshis,
                payment.Fee?.Satoshis,
                payment.BitcoinTxId?.Value,
                payment.ConfirmationCount,
                broadcastResult.ExplorerUrl ?? settings.BuildExplorerUrl(payment.BitcoinTxId?.Value),
                payment.CreatedAt,
                buildResult.Escrow.EscrowAddress.Value,
                buildResult.Escrow.ExpiresAtBlock);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            payment.MarkFailed(ex.Message);
            await transactionRepository.UpdateAsync(payment, cancellationToken);
            throw;
        }
    }

    private async Task<Wallet> GetRequiredWalletAsync(Guid walletId, CancellationToken cancellationToken) =>
        await walletRepository.GetByIdAsync(walletId, cancellationToken)
        ?? throw new InvalidOperationStateException($"Wallet '{walletId}' was not found.");
}
