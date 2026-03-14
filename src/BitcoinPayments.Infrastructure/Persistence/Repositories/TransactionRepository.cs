using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPayments.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core payment transaction repository.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionRepository"/> class.
    /// </summary>
    public TransactionRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(PaymentTransaction transaction, CancellationToken cancellationToken)
    {
        await dbContext.PaymentTransactions.AddAsync(transaction, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<PaymentTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.PaymentTransactions.FirstOrDefaultAsync(transaction => transaction.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<PaymentTransaction?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken) =>
        dbContext.PaymentTransactions.FirstOrDefaultAsync(
            transaction => transaction.IdempotencyKey == idempotencyKey,
            cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<PaymentTransaction>> GetChildrenAsync(Guid parentTransactionId, CancellationToken cancellationToken) =>
        await dbContext.PaymentTransactions
            .Where(transaction => transaction.ParentTransactionId == parentTransactionId)
            .OrderBy(transaction => transaction.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<PaymentTransaction>> ListPendingAsync(CancellationToken cancellationToken) =>
        await dbContext.PaymentTransactions
            .Where(transaction => transaction.BitcoinTxId != null)
            .Where(transaction => transaction.State != TransactionState.Settled &&
                                  transaction.State != TransactionState.Voided &&
                                  transaction.State != TransactionState.Expired &&
                                  transaction.State != TransactionState.Failed)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task UpdateAsync(PaymentTransaction transaction, CancellationToken cancellationToken)
    {
        dbContext.PaymentTransactions.Update(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
