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
/// Handles refund requests.
/// </summary>
public sealed class RefundHandler : IRequestHandler<CreateRefundCommand, PaymentResponse>
{
    private readonly IWalletRepository walletRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IUtxoRepository utxoRepository;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly WalletSynchronizationService walletSynchronizationService;
    private readonly TransactionBuilderService transactionBuilderService;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefundHandler"/> class.
    /// </summary>
    public RefundHandler(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IUtxoRepository utxoRepository,
        IBitcoinNetwork bitcoinNetwork,
        WalletSynchronizationService walletSynchronizationService,
        TransactionBuilderService transactionBuilderService,
        IBitcoinSettings settings)
    {
        this.walletRepository = walletRepository;
        this.transactionRepository = transactionRepository;
        this.utxoRepository = utxoRepository;
        this.bitcoinNetwork = bitcoinNetwork;
        this.walletSynchronizationService = walletSynchronizationService;
        this.transactionBuilderService = transactionBuilderService;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> Handle(CreateRefundCommand request, CancellationToken cancellationToken)
    {
        var parentTransaction = await transactionRepository.GetByIdAsync(request.ParentTransactionId, cancellationToken)
                                 ?? throw new InvalidOperationStateException($"Transaction '{request.ParentTransactionId}' was not found.");

        if (parentTransaction.State != TransactionState.Settled)
        {
            throw new InvalidOperationStateException("Refunds are only allowed after settlement.");
        }

        var existingRefunds = await transactionRepository.GetChildrenAsync(parentTransaction.Id, cancellationToken);
        var totalRefunded = existingRefunds
            .Where(child => child.OperationType == PaymentOperationType.Refund)
            .Aggregate(Money.Zero, static (current, child) => current + child.Amount);

        var requestedAmount = Money.FromSatoshis(request.AmountSatoshis);
        if (totalRefunded + requestedAmount > parentTransaction.Amount)
        {
            throw new InvalidOperationStateException("Refund amount exceeds the original settled amount.");
        }

        var merchantWallet = await GetRequiredWalletAsync(parentTransaction.MerchantWalletId, cancellationToken);
        var buyerWallet = await GetRequiredWalletAsync(parentTransaction.BuyerWalletId, cancellationToken);

        await walletSynchronizationService.SynchronizeAsync(merchantWallet, cancellationToken);
        var merchantUtxos = await utxoRepository.GetUnspentByWalletIdAsync(merchantWallet.Id, cancellationToken);

        var refund = new PaymentTransaction(
            request.IdempotencyKey,
            PaymentOperationType.Refund,
            requestedAmount,
            parentTransaction.BuyerWalletId,
            parentTransaction.MerchantWalletId)
        {
            Metadata = request.Metadata ?? new Dictionary<string, string?>(),
        };

        refund.AttachToParent(parentTransaction.Id);
        await transactionRepository.AddAsync(refund, cancellationToken);

        try
        {
            var builtTransaction = transactionBuilderService.BuildRefund(
                merchantWallet,
                buyerWallet,
                merchantUtxos,
                requestedAmount,
                request.FeeRateSatPerByte);

            var broadcastResult = await bitcoinNetwork.BroadcastTransactionAsync(
                builtTransaction.RawTransactionHex,
                cancellationToken);

            refund.MarkBroadcast(
                broadcastResult.TransactionId,
                builtTransaction.RawTransactionHex,
                builtTransaction.Fee,
                TransactionState.Mempool);

            foreach (var utxo in builtTransaction.ConsumedUtxos)
            {
                utxo.MarkSpent(broadcastResult.TransactionId);
                await utxoRepository.UpdateAsync(utxo, cancellationToken);
            }

            foreach (var output in builtTransaction.Outputs)
            {
                await utxoRepository.AddOrUpdateAsync(
                    new Utxo
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
                    },
                    cancellationToken);
            }

            await walletRepository.UpdateAsync(merchantWallet, cancellationToken);
            await walletRepository.UpdateAsync(buyerWallet, cancellationToken);
            await transactionRepository.UpdateAsync(refund, cancellationToken);

            return PaymentResponse.FromEntity(refund, broadcastResult.ExplorerUrl ?? settings.BuildExplorerUrl(refund.BitcoinTxId?.Value));
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            refund.MarkFailed(ex.Message);
            await transactionRepository.UpdateAsync(refund, cancellationToken);
            throw;
        }
    }

    private async Task<Wallet> GetRequiredWalletAsync(Guid walletId, CancellationToken cancellationToken) =>
        await walletRepository.GetByIdAsync(walletId, cancellationToken)
        ?? throw new InvalidOperationStateException($"Wallet '{walletId}' was not found.");
}
