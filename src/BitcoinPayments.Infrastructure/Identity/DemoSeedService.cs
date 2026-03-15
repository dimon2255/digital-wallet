using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.ValueObjects;
using BitcoinPayments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BitcoinPayments.Infrastructure.Identity;

/// <summary>
/// Seeds a demo user with wallets and sample transactions for reviewer experience.
/// </summary>
public static class DemoSeedService
{
    /// <summary>
    /// The demo user email address.
    /// </summary>
    public const string DemoEmail = "demo@chainvault.dev";

    /// <summary>
    /// The demo user password.
    /// </summary>
    public const string DemoPassword = "Demo123!";

    /// <summary>
    /// Seeds demo data idempotently.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var walletKeyService = scope.ServiceProvider.GetRequiredService<IWalletKeyService>();
        var settings = scope.ServiceProvider.GetRequiredService<IBitcoinSettings>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        var existing = await userManager.FindByEmailAsync(DemoEmail);
        if (existing is not null)
        {
            return;
        }

        logger.LogInformation("Seeding demo user and sample data...");

        var demoUser = new ApplicationUser
        {
            UserName = DemoEmail,
            Email = DemoEmail,
            DisplayName = "Demo User",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await userManager.CreateAsync(demoUser, DemoPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to create demo user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        // Create wallets
        var wallet1 = walletKeyService.CreateWallet("Personal Wallet", settings.Network);
        wallet1.UserId = demoUser.Id;

        var wallet2 = walletKeyService.CreateWallet("Business Wallet", settings.Network);
        wallet2.UserId = demoUser.Id;

        var wallet3 = walletKeyService.CreateWallet("Savings Vault", settings.Network);
        wallet3.UserId = demoUser.Id;

        await dbContext.Wallets.AddRangeAsync(wallet1, wallet2, wallet3);

        // Create sample transactions
        var sampleTransactions = new[]
        {
            new PaymentTransaction(
                null, PaymentOperationType.Charge, Money.FromSatoshis(50000),
                wallet1.Id, wallet2.Id)
            {
                State = TransactionState.Settled,
                ConfirmationCount = 6,
                Fee = Money.FromSatoshis(354),
                BitcoinTxId = new TransactionId("a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2"),
            },
            new PaymentTransaction(
                null, PaymentOperationType.Transfer, Money.FromSatoshis(25000),
                wallet2.Id, wallet3.Id)
            {
                State = TransactionState.TransferCompleted,
                Fee = Money.FromSatoshis(0),
            },
            new PaymentTransaction(
                null, PaymentOperationType.Charge, Money.FromSatoshis(100000),
                wallet1.Id, wallet2.Id)
            {
                State = TransactionState.Confirming,
                ConfirmationCount = 2,
                Fee = Money.FromSatoshis(512),
                BitcoinTxId = new TransactionId("f6e5d4c3b2a1f6e5d4c3b2a1f6e5d4c3b2a1f6e5d4c3b2a1f6e5d4c3b2a1f6e5"),
            },
        };

        await dbContext.PaymentTransactions.AddRangeAsync(sampleTransactions);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Demo seed complete: user={Email}, wallets=3, transactions={Count}.", DemoEmail, sampleTransactions.Length);
    }
}
