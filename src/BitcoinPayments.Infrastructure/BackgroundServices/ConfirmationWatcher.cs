using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Enums;
using BitcoinPayments.Domain.Interfaces;
using BitcoinPayments.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitcoinPayments.Infrastructure.BackgroundServices;

/// <summary>
/// Watches pending transactions and updates confirmation state.
/// </summary>
public sealed class ConfirmationWatcher : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IBitcoinNetwork bitcoinNetwork;
    private readonly IBitcoinSettings settings;
    private readonly BackgroundServiceOptions backgroundServiceOptions;
    private readonly ILogger<ConfirmationWatcher> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmationWatcher"/> class.
    /// </summary>
    public ConfirmationWatcher(
        IServiceScopeFactory serviceScopeFactory,
        IBitcoinNetwork bitcoinNetwork,
        IBitcoinSettings settings,
        IOptions<BackgroundServiceOptions> backgroundServiceOptions,
        ILogger<ConfirmationWatcher> logger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.bitcoinNetwork = bitcoinNetwork;
        this.settings = settings;
        this.backgroundServiceOptions = backgroundServiceOptions.Value;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(backgroundServiceOptions.ConfirmationWatcherIntervalSeconds));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var escrowRepository = scope.ServiceProvider.GetRequiredService<IEscrowRepository>();
            var pendingTransactions = await transactionRepository.ListPendingAsync(stoppingToken);
            foreach (var transaction in pendingTransactions)
            {
                if (transaction.BitcoinTxId is null)
                {
                    continue;
                }

                try
                {
                    var status = await bitcoinNetwork.GetTransactionStatusAsync(transaction.BitcoinTxId.Value, stoppingToken);
                    transaction.UpdateConfirmations(status.ConfirmationCount, settings.MinConfirmationsForSettlement);
                    await transactionRepository.UpdateAsync(transaction, stoppingToken);

                    if (transaction.OperationType == Domain.ValueObjects.PaymentOperationType.Auth && status.Confirmed)
                    {
                        var escrow = await escrowRepository.GetByPaymentTransactionIdAsync(transaction.Id, stoppingToken);
                        if (escrow is not null && escrow.State == EscrowState.Created && escrow.FundingUtxoId.HasValue)
                        {
                            escrow.MarkFunded(escrow.FundingUtxoId.Value, escrow.ExpiresAtBlock ?? 0);
                            await escrowRepository.UpdateAsync(escrow, stoppingToken);
                        }
                    }

                    if (status.Confirmed && transaction.ParentTransactionId.HasValue)
                    {
                        var parent = await transactionRepository.GetByIdAsync(transaction.ParentTransactionId.Value, stoppingToken);
                        if (parent is not null)
                        {
                            if (transaction.OperationType == Domain.ValueObjects.PaymentOperationType.Capture)
                            {
                                parent.TransitionTo(TransactionState.Settled);
                                await transactionRepository.UpdateAsync(parent, stoppingToken);
                            }
                            else if (transaction.OperationType == Domain.ValueObjects.PaymentOperationType.Void)
                            {
                                parent.TransitionTo(TransactionState.Voided);
                                await transactionRepository.UpdateAsync(parent, stoppingToken);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to update confirmation state for payment {PaymentId}.", transaction.Id);
                }
            }
        }
    }
}
