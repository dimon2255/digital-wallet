using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Domain.ValueObjects;

namespace BitcoinPayments.Application.Services.Transfers;

/// <summary>
/// Executes an instant internal ledger transfer (no on-chain transaction).
/// </summary>
public sealed class InternalLedgerStrategy : ITransferStrategy
{
    private readonly ITransactionRepository transactionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalLedgerStrategy"/> class.
    /// </summary>
    public InternalLedgerStrategy(ITransactionRepository transactionRepository)
    {
        this.transactionRepository = transactionRepository;
    }

    /// <inheritdoc />
    public async Task<PaymentTransaction> ExecuteAsync(TransferContext context, CancellationToken cancellationToken)
    {
        var transaction = new PaymentTransaction(
            context.IdempotencyKey,
            PaymentOperationType.Transfer,
            Money.FromSatoshis(context.AmountSatoshis),
            context.SourceWallet.Id,
            context.DestinationWallet.Id)
        {
            State = TransactionState.TransferCompleted,
            Fee = Money.FromSatoshis(0),
        };

        await transactionRepository.AddAsync(transaction, cancellationToken);
        return transaction;
    }
}
