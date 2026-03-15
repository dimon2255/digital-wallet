using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.DTOs;
using BitcoinPayments.Application.Services.Transfers;
using BitcoinPayments.Domain.Exceptions;
using BitcoinPayments.Domain.Interfaces;
using MediatR;

namespace BitcoinPayments.Application.Handlers;

/// <summary>
/// Handles wallet-to-wallet transfer commands.
/// </summary>
public sealed class TransferHandler : IRequestHandler<CreateTransferCommand, PaymentResponse>
{
    private readonly IWalletRepository walletRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly TransferStrategyResolver strategyResolver;
    private readonly IBitcoinSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransferHandler"/> class.
    /// </summary>
    public TransferHandler(
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        TransferStrategyResolver strategyResolver,
        IBitcoinSettings settings)
    {
        this.walletRepository = walletRepository;
        this.transactionRepository = transactionRepository;
        this.strategyResolver = strategyResolver;
        this.settings = settings;
    }

    /// <inheritdoc />
    public async Task<PaymentResponse> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        // Check idempotency
        if (request.IdempotencyKey is not null)
        {
            var existing = await transactionRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
            if (existing is not null)
            {
                return PaymentResponse.FromEntity(existing, settings.BuildExplorerUrl(existing.BitcoinTxId?.Value));
            }
        }

        var sourceWallet = await walletRepository.GetByIdAsync(request.SourceWalletId, cancellationToken)
            ?? throw new InvalidOperationStateException($"Source wallet '{request.SourceWalletId}' not found.");

        if (sourceWallet.UserId != request.UserId)
        {
            throw new ForbiddenAccessException("Source wallet does not belong to the current user.");
        }

        var destWallet = await walletRepository.GetByIdAsync(request.DestinationWalletId, cancellationToken)
            ?? throw new InvalidOperationStateException($"Destination wallet '{request.DestinationWalletId}' not found.");

        var strategy = strategyResolver.Resolve(sourceWallet, destWallet);
        var context = new TransferContext(sourceWallet, destWallet, request.AmountSatoshis, request.FeeRateSatPerByte, request.IdempotencyKey);
        var transaction = await strategy.ExecuteAsync(context, cancellationToken);

        return PaymentResponse.FromEntity(transaction, settings.BuildExplorerUrl(transaction.BitcoinTxId?.Value));
    }
}
