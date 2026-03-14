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
/// Handles authorization void requests.
/// </summary>
public sealed class VoidHandler : IRequestHandler<VoidAuthCommand, PaymentResponse>
{
    private readonly IWalletRepository walletRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IUtxoRepository utxoRepository;
    private readonly IEscrowRepository escrowRepository;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly EscrowService escrowService;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="VoidHandler"/> class.
    /// </summary>
    public VoidHandler(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        IUtxoRepository utxoRepository,
        IEscrowRepository escrowRepository,
        IBitcoinNetwork bitcoinNetwork,
        EscrowService escrowService,
        IBitcoinSettings settings)
    {
        this.walletRepository = walletRepository;
        this.transactionRepository = transactionRepository;
        this.utxoRepository = utxoRepository;
        this.escrowRepository = escrowRepository;
        this.bitcoinNetwork = bitcoinNetwork;
        this.escrowService = escrowService;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> Handle(VoidAuthCommand request, CancellationToken cancellationToken)
    {
        var authorization = await transactionRepository.GetByIdAsync(request.AuthorizationId, cancellationToken)
                            ?? throw new InvalidOperationStateException($"Authorization '{request.AuthorizationId}' was not found.");

        if (authorization.OperationType != PaymentOperationType.Auth)
        {
            throw new InvalidOperationStateException("Void can only be created from an authorization.");
        }

        if (authorization.State == TransactionState.CaptureBroadcast || authorization.State == TransactionState.Settled)
        {
            throw new InvalidOperationStateException($"Authorization '{authorization.Id}' has already been captured.");
        }

        if (authorization.State == TransactionState.Expired || authorization.State == TransactionState.Voided)
        {
            throw new InvalidOperationStateException($"Authorization '{authorization.Id}' can no longer be voided.");
        }

        var escrow = await escrowRepository.GetByPaymentTransactionIdAsync(authorization.Id, cancellationToken)
                     ?? throw new EscrowNotFoundException($"Escrow for authorization '{authorization.Id}' was not found.");

        var buyerWallet = await GetRequiredWalletAsync(authorization.BuyerWalletId, cancellationToken);
        var merchantWallet = await GetRequiredWalletAsync(authorization.MerchantWalletId, cancellationToken);
        var escrowUtxo = escrow.FundingUtxoId is null
            ? throw new EscrowNotFoundException($"Escrow funding UTXO for authorization '{authorization.Id}' was not found.")
            : await utxoRepository.GetByIdAsync(escrow.FundingUtxoId.Value, cancellationToken)
              ?? throw new EscrowNotFoundException($"Escrow funding UTXO for authorization '{authorization.Id}' was not found.");

        var voidTransaction = new Domain.Entities.PaymentTransaction(
            request.IdempotencyKey,
            Domain.ValueObjects.PaymentOperationType.Void,
            authorization.Amount,
            authorization.BuyerWalletId,
            authorization.MerchantWalletId)
        {
            Metadata = request.Metadata ?? new Dictionary<string, string?>(),
        };

        voidTransaction.AttachToParent(authorization.Id);
        await transactionRepository.AddAsync(voidTransaction, cancellationToken);

        try
        {
            var builtTransaction = escrowService.BuildVoid(
                buyerWallet,
                merchantWallet,
                escrow,
                escrowUtxo,
                request.FeeRateSatPerByte);

            var broadcastResult = await bitcoinNetwork.BroadcastTransactionAsync(
                builtTransaction.RawTransactionHex,
                cancellationToken);

            voidTransaction.MarkBroadcast(
                broadcastResult.TransactionId,
                builtTransaction.RawTransactionHex,
                builtTransaction.Fee,
                TransactionState.VoidBroadcast);

            escrowUtxo.MarkSpent(broadcastResult.TransactionId);
            await utxoRepository.UpdateAsync(escrowUtxo, cancellationToken);

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

            escrow.MarkVoided();
            authorization.TransitionTo(TransactionState.VoidBroadcast);

            await escrowRepository.UpdateAsync(escrow, cancellationToken);
            await walletRepository.UpdateAsync(buyerWallet, cancellationToken);
            await walletRepository.UpdateAsync(merchantWallet, cancellationToken);
            await transactionRepository.UpdateAsync(authorization, cancellationToken);
            await transactionRepository.UpdateAsync(voidTransaction, cancellationToken);

            return PaymentResponse.FromEntity(voidTransaction, broadcastResult.ExplorerUrl ?? settings.BuildExplorerUrl(voidTransaction.BitcoinTxId?.Value));
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            voidTransaction.MarkFailed(ex.Message);
            await transactionRepository.UpdateAsync(voidTransaction, cancellationToken);
            throw;
        }
    }

    private async Task<Wallet> GetRequiredWalletAsync(Guid walletId, CancellationToken cancellationToken) =>
        await walletRepository.GetByIdAsync(walletId, cancellationToken)
        ?? throw new InvalidOperationStateException($"Wallet '{walletId}' was not found.");
}
