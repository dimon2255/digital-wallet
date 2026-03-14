using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitcoinPayments.Infrastructure.BackgroundServices;

/// <summary>
/// Marks funded escrows as expired when their block window elapses.
/// </summary>
public sealed class AuthExpiryMonitor : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly BackgroundServiceOptions backgroundServiceOptions;
    private readonly ILogger<AuthExpiryMonitor> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthExpiryMonitor"/> class.
    /// </summary>
    public AuthExpiryMonitor(
        IServiceScopeFactory serviceScopeFactory,
        IBitcoinNetwork bitcoinNetwork,
        IOptions<BackgroundServiceOptions> backgroundServiceOptions,
        ILogger<AuthExpiryMonitor> logger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.bitcoinNetwork = bitcoinNetwork;
        this.backgroundServiceOptions = backgroundServiceOptions.Value;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(backgroundServiceOptions.AuthExpiryCheckIntervalSeconds));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var escrowRepository = scope.ServiceProvider.GetRequiredService<IEscrowRepository>();
                var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var currentHeight = await bitcoinNetwork.GetCurrentBlockHeightAsync(stoppingToken);
                var activeEscrows = await escrowRepository.ListActiveAsync(stoppingToken);
                foreach (var escrow in activeEscrows.Where(candidate => candidate.State == EscrowState.Funded))
                {
                    if (!escrow.ExpiresAtBlock.HasValue || escrow.ExpiresAtBlock.Value > currentHeight)
                    {
                        continue;
                    }

                    escrow.MarkExpired();
                    await escrowRepository.UpdateAsync(escrow, stoppingToken);

                    var transaction = await transactionRepository.GetByIdAsync(escrow.PaymentTransactionId, stoppingToken);
                    if (transaction is not null &&
                        transaction.State != TransactionState.Settled &&
                        transaction.State != TransactionState.Voided)
                    {
                        transaction.TransitionTo(TransactionState.Expired);
                        await transactionRepository.UpdateAsync(transaction, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed while monitoring authorization expiry.");
            }
        }
    }
}
