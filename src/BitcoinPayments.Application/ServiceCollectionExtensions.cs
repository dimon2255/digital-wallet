using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Services;
using BitcoinPayments.Application.Services.Transfers;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BitcoinPayments.Application;

/// <summary>
/// Configures application-layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers application services.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ValidationBehavior<,>).Assembly;
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<CoinSelectionService>();
        services.AddScoped<FeeEstimationService>();
        services.AddScoped<TransactionBuilderService>();
        services.AddScoped<EscrowService>();
        services.AddScoped<WalletSynchronizationService>();
        services.AddScoped<IWalletApplicationService, WalletApplicationService>();
        services.AddScoped<IPaymentQueryService, PaymentQueryService>();

        // Transfer services
        services.AddScoped<InternalLedgerStrategy>();
        services.AddScoped<OnChainTransferStrategy>();
        services.AddScoped<TransferStrategyResolver>();

        return services;
    }
}
