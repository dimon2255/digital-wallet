using BitcoinPayments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPayments.Infrastructure.Persistence;

/// <summary>
/// Entity Framework database context for the platform.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the wallets set.
    /// </summary>
    public DbSet<Wallet> Wallets => Set<Wallet>();

    /// <summary>
    /// Gets the UTXOs set.
    /// </summary>
    public DbSet<Utxo> Utxos => Set<Utxo>();

    /// <summary>
    /// Gets the payment transactions set.
    /// </summary>
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    /// <summary>
    /// Gets the escrows set.
    /// </summary>
    public DbSet<Escrow> Escrows => Set<Escrow>();

    /// <summary>
    /// Gets the idempotency records set.
    /// </summary>
    public DbSet<IdempotencyKeyRecord> IdempotencyKeys => Set<IdempotencyKeyRecord>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
