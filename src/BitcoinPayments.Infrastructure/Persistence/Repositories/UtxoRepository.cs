using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPayments.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core UTXO repository.
/// </summary>
public sealed class UtxoRepository : IUtxoRepository
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UtxoRepository"/> class.
    /// </summary>
    public UtxoRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddOrUpdateAsync(Utxo utxo, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Utxos.FirstOrDefaultAsync(
            candidate => candidate.TransactionId == utxo.TransactionId && candidate.OutputIndex == utxo.OutputIndex,
            cancellationToken);

        if (existing is null)
        {
            await dbContext.Utxos.AddAsync(utxo, cancellationToken);
        }
        else
        {
            existing.WalletId = utxo.WalletId;
            existing.Amount = utxo.Amount;
            existing.ScriptPubKey = utxo.ScriptPubKey;
            existing.Address = utxo.Address;
            existing.IsSpent = utxo.IsSpent;
            existing.SpentByTx = utxo.SpentByTx;
            existing.ConfirmationCount = utxo.ConfirmationCount;
            existing.IsEscrow = utxo.IsEscrow;
            existing.DerivationIndex = utxo.DerivationIndex;
            existing.IsChangeAddress = utxo.IsChangeAddress;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<Utxo> utxos, CancellationToken cancellationToken)
    {
        await dbContext.Utxos.AddRangeAsync(utxos, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Utxo?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Utxos.FirstOrDefaultAsync(utxo => utxo.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Utxo>> GetByWalletIdAsync(Guid walletId, CancellationToken cancellationToken) =>
        await dbContext.Utxos
            .Where(utxo => utxo.WalletId == walletId)
            .OrderByDescending(utxo => utxo.CreatedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Utxo>> GetUnspentByWalletIdAsync(Guid walletId, CancellationToken cancellationToken) =>
        await dbContext.Utxos
            .Where(utxo => utxo.WalletId == walletId && !utxo.IsSpent)
            .OrderByDescending(utxo => utxo.Amount.Satoshis)
            .ThenByDescending(utxo => utxo.ConfirmationCount)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task UpdateAsync(Utxo utxo, CancellationToken cancellationToken)
    {
        dbContext.Utxos.Update(utxo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
