using System.Text;
using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Infrastructure.BackgroundServices;
using BitcoinPayments.Infrastructure.Bitcoin;
using BitcoinPayments.Infrastructure.Configuration;
using BitcoinPayments.Infrastructure.Identity;
using BitcoinPayments.Infrastructure.Persistence;
using BitcoinPayments.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;

namespace BitcoinPayments.Infrastructure.Extensions;

/// <summary>
/// Configures infrastructure-layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers infrastructure services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BitcoinOptions>(configuration.GetSection("Bitcoin"));
        services.Configure<BackgroundServiceOptions>(configuration.GetSection("BackgroundServices"));

        services.AddDataProtection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Identity
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // JWT
        var jwtSection = configuration.GetSection("Jwt");
        services.Configure<JwtOptions>(jwtSection);
        var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();
        services.AddScoped<JwtTokenService>();

        // Repositories
        services.AddScoped<BitcoinPayments.Domain.Interfaces.IWalletRepository, WalletRepository>();
        services.AddScoped<BitcoinPayments.Domain.Interfaces.ITransactionRepository, TransactionRepository>();
        services.AddScoped<BitcoinPayments.Domain.Interfaces.IUtxoRepository, UtxoRepository>();
        services.AddScoped<BitcoinPayments.Domain.Interfaces.IEscrowRepository, EscrowRepository>();
        services.AddScoped<BitcoinPayments.Domain.Interfaces.IIdempotencyKeyRepository, IdempotencyKeyRepository>();

        services.AddScoped<IWalletKeyService, HdWalletManager>();
        services.AddSingleton<IBitcoinSettings, BitcoinSettings>();
        services.AddSingleton<ScriptBuilder>();
        services.AddSingleton<TestnetBroadcaster>();
        services.AddSingleton<BitcoinPayments.Domain.Interfaces.IBitcoinNetwork, NBitcoinNetworkService>();

        services.AddHttpClient(nameof(NBitcoinNetworkService), (serviceProvider, client) =>
            {
                var options = configuration.GetSection("Bitcoin").Get<BitcoinOptions>() ?? new BitcoinOptions();
                client.BaseAddress = new Uri(options.MempoolApiBaseUrl.TrimEnd('/') + "/");
            })
            .AddStandardResilienceHandler();

        services.AddHostedService<ConfirmationWatcher>();
        services.AddHostedService<AuthExpiryMonitor>();

        return services;
    }
}
