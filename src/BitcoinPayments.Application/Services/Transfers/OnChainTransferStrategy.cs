using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Application.Services.Transfers;

/// <summary>
/// Executes an on-chain transfer by building and broadcasting a Bitcoin transaction.
/// </summary>
public sealed class OnChainTransferStrategy : ITransferStrategy
{
    private readonly TransactionBuilderService transactionBuilderService;
    private readonly ITransactionRepository transactionRepository;
    private readonly IUtxoRepository utxoRepository;
    private readonly IWalletRepository walletRepository;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly WalletSynchronizationService walletSyncService;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnChainTransferStrategy"/> class.
    /// </summary>
    public OnChainTransferStrategy(
        TransactionBuilderService transactionBuilderService,
        ITransactionRepository transactionRepository,
        IUtxoRepository utxoRepository,
        IWalletRepository walletRepository,
        IBitcoinNetwork bitcoinNetwork,
        WalletSynchronizationService walletSyncService,
        IBitcoinSettings settings)
    {
        this.transactionBuilderService = transactionBuilderService;
        this.transactionRepository = transactionRepository;
        this.utxoRepository = utxoRepository;
        this.walletRepository = walletRepository;
        this.bitcoinNetwork = bitcoinNetwork;
        this.walletSyncService = walletSyncService;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<PaymentTransaction> ExecuteAsync(TransferContext context, CancellationToken cancellationToken)
    {
        await walletSyncService.SynchronizeAsync(context.SourceWallet, cancellationToken);
        var availableUtxos = await utxoRepository.GetUnspentByWalletIdAsync(context.SourceWallet.Id, cancellationToken);

        var payment = new PaymentTransaction(
            context.IdempotencyKey,
            PaymentOperationType.Transfer,
            Money.FromSatoshis(context.AmountSatoshis),
            context.SourceWallet.Id,
            context.DestinationWallet.Id);

        await transactionRepository.AddAsync(payment, cancellationToken);

        var builtTx = transactionBuilderService.BuildCharge(
            context.SourceWallet,
            context.DestinationWallet,
            availableUtxos,
            Money.FromSatoshis(context.AmountSatoshis),
            context.FeeRateSatPerByte);

        var broadcastResult = await bitcoinNetwork.BroadcastTransactionAsync(builtTx.RawTransactionHex, cancellationToken);

        payment.MarkBroadcast(broadcastResult.TransactionId, builtTx.RawTransactionHex, builtTx.Fee, TransactionState.Mempool);

        foreach (var utxo in builtTx.ConsumedUtxos)
        {
            utxo.MarkSpent(broadcastResult.TransactionId);
            await utxoRepository.UpdateAsync(utxo, cancellationToken);
        }

        foreach (var output in builtTx.Outputs)
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

        await walletRepository.UpdateAsync(context.SourceWallet, cancellationToken);
        await walletRepository.UpdateAsync(context.DestinationWallet, cancellationToken);
        await transactionRepository.UpdateAsync(payment, cancellationToken);

        return payment;
    }
}
