using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;

namespace BitcoinPayments.Application.Services.Transfers;

/// <summary>
/// Resolves the appropriate transfer strategy based on configuration and wallet ownership.
/// </summary>
public sealed class TransferStrategyResolver
{
    private readonly IBitcoinSettings settings;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransferStrategyResolver"/> class.
    /// </summary>
    public TransferStrategyResolver(IBitcoinSettings settings, IServiceProvider serviceProvider)
    {
        this.settings = settings;
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Resolves the transfer strategy for the given wallets.
    /// </summary>
    public ITransferStrategy Resolve(Wallet source, Wallet destination)
    {
        var mode = settings.TransferMode.ToLowerInvariant();

        if (mode == "internal")
        {
            return (ITransferStrategy)serviceProvider.GetService(typeof(InternalLedgerStrategy))!;
        }

        if (mode == "onchain")
        {
            return (ITransferStrategy)serviceProvider.GetService(typeof(OnChainTransferStrategy))!;
        }

        // Auto: internal if same user, on-chain otherwise
        if (source.UserId is not null && source.UserId == destination.UserId)
        {
            return (ITransferStrategy)serviceProvider.GetService(typeof(InternalLedgerStrategy))!;
        }

        return (ITransferStrategy)serviceProvider.GetService(typeof(OnChainTransferStrategy))!;
    }
}
