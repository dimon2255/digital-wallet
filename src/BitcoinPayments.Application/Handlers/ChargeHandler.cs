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
/// Handles direct charge requests.
/// </summary>
public sealed class ChargeHandler : IRequestHandler<CreateChargeCommand, PaymentResponse>
{
    private readonly IWalletRepository walletRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IUtxoRepository utxoRepository;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly TransactionBuilderService transactionBuilderService;
    private readonly WalletSynchronizationService walletSynchronizationService;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChargeHandler"/> class.
    /// </summary>
    public ChargeHandler(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IUtxoRepository utxoRepository,
        IBitcoinNetwork bitcoinNetwork,
        TransactionBuilderService transactionBuilderService,
        WalletSynchronizationService walletSynchronizationService,
        IBitcoinSettings settings)
    {
        this.walletRepository = walletRepository;
        this.transactionRepository = transactionRepository;
        this.utxoRepository = utxoRepository;
        this.bitcoinNetwork = bitcoinNetwork;
        this.transactionBuilderService = transactionBuilderService;
        this.walletSynchronizationService = walletSynchronizationService;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> Handle(CreateChargeCommand request, CancellationToken cancellationToken)
    {
        var buyerWallet = await GetRequiredWalletAsync(request.BuyerWalletId, cancellationToken);
        var merchantWallet = await GetRequiredWalletAsync(request.MerchantWalletId, cancellationToken);

        await walletSynchronizationService.SynchronizeAsync(buyerWallet, cancellationToken);
        var availableUtxos = await utxoRepository.GetUnspentByWalletIdAsync(buyerWallet.Id, cancellationToken);

        var payment = new PaymentTransaction(
            request.IdempotencyKey,
            PaymentOperationType.Charge,
            Money.FromSatoshis(request.AmountSatoshis),
            buyerWallet.Id,
            merchantWallet.Id)
        {
            Metadata = request.Metadata ?? new Dictionary<string, string?>(),
        };

        await transactionRepository.AddAsync(payment, cancellationToken);

        try
        {
            var builtTransaction = transactionBuilderService.BuildCharge(
                buyerWallet,
                merchantWallet,
                availableUtxos,
                Money.FromSatoshis(request.AmountSatoshis),
                request.FeeRateSatPerByte);

            var broadcastResult = await bitcoinNetwork.BroadcastTransactionAsync(
                builtTransaction.RawTransactionHex,
                cancellationToken);

            payment.MarkBroadcast(
                broadcastResult.TransactionId,
                builtTransaction.RawTransactionHex,
                builtTransaction.Fee,
                TransactionState.Mempool);

            foreach (var utxo in builtTransaction.ConsumedUtxos)
            {
                utxo.MarkSpent(broadcastResult.TransactionId);
                await utxoRepository.UpdateAsync(utxo, cancellationToken);
            }

            await PersistOutputsAsync(builtTransaction, broadcastResult.TransactionId, cancellationToken);
            await walletRepository.UpdateAsync(buyerWallet, cancellationToken);
            await walletRepository.UpdateAsync(merchantWallet, cancellationToken);
            await transactionRepository.UpdateAsync(payment, cancellationToken);

            return PaymentResponse.FromEntity(payment, broadcastResult.ExplorerUrl ?? settings.BuildExplorerUrl(payment.BitcoinTxId?.Value));
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            payment.MarkFailed(ex.Message);
            await transactionRepository.UpdateAsync(payment, cancellationToken);
            throw;
        }
    }

    private async Task PersistOutputsAsync(BuiltTransaction builtTransaction, TransactionId spendingTxId, CancellationToken cancellationToken)
    {
        foreach (var output in builtTransaction.Outputs)
        {
            await utxoRepository.AddOrUpdateAsync(
                new Utxo
                {
                    WalletId = output.WalletId,
                    TransactionId = spendingTxId,
                    OutputIndex = output.OutputIndex,
                    Amount = output.Amount,
                    ScriptPubKey = output.ScriptPubKeyHex,
                    Address = output.Address,
                    ConfirmationCount = 0,
                    IsEscrow = output.IsEscrow,
                    DerivationIndex = output.DerivationIndex,
                    IsChangeAddress = output.IsChange,
                },
                cancellationToken);
        }
    }

    private async Task<Wallet> GetRequiredWalletAsync(Guid walletId, CancellationToken cancellationToken) =>
        await walletRepository.GetByIdAsync(walletId, cancellationToken)
        ?? throw new InvalidOperationStateException($"Wallet '{walletId}' was not found.");
}
