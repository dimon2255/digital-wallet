using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPayments.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core escrow repository.
/// </summary>
public sealed class EscrowRepository : IEscrowRepository
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="EscrowRepository"/> class.
    /// </summary>
    public EscrowRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(Escrow escrow, CancellationToken cancellationToken)
    {
        await dbContext.Escrows.AddAsync(escrow, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Escrow?> GetByIdAsync(Guid escrowId, CancellationToken cancellationToken) =>
        dbContext.Escrows.FirstOrDefaultAsync(escrow => escrow.Id == escrowId, cancellationToken);

    /// <inheritdoc />
    public Task<Escrow?> GetByPaymentTransactionIdAsync(Guid paymentTransactionId, CancellationToken cancellationToken) =>
        dbContext.Escrows.FirstOrDefaultAsync(escrow => escrow.PaymentTransactionId == paymentTransactionId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Escrow>> ListActiveAsync(CancellationToken cancellationToken) =>
        await dbContext.Escrows
            .Where(escrow => escrow.State == EscrowState.Created || escrow.State == EscrowState.Funded)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task UpdateAsync(Escrow escrow, CancellationToken cancellationToken)
    {
        dbContext.Escrows.Update(escrow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
