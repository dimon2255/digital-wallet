using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPayments.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core wallet repository.
/// </summary>
public sealed class WalletRepository : IWalletRepository
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalletRepository"/> class.
    /// </summary>
    public WalletRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        await dbContext.Wallets.AddAsync(wallet, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Wallets.FirstOrDefaultAsync(wallet => wallet.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Wallet?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
        dbContext.Wallets.FirstOrDefaultAsync(wallet => wallet.Name == name, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Wallet>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Wallets.OrderBy(wallet => wallet.Name).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken)
    {
        dbContext.Wallets.Update(wallet);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
