using BitcoinPayments.Application.DTOs;

namespace BitcoinPayments.Application.Abstractions;

/// <summary>
/// Exposes wallet-oriented use cases to the API layer.
/// </summary>
public interface IWalletApplicationService
{
    /// <summary>
    /// Creates a new wallet for a user.
    /// </summary>
    Task<WalletResponse> CreateWalletAsync(string name, string? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a wallet summary with ownership check.
    /// </summary>
    Task<WalletResponse> GetWalletAsync(Guid walletId, string? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists wallets belonging to a user.
    /// </summary>
    Task<IReadOnlyCollection<WalletResponse>> ListWalletsAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Generates the next receiving address.
    /// </summary>
    Task<WalletAddressResponse> GetNextReceivingAddressAsync(Guid walletId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists tracked UTXOs for a wallet.
    /// </summary>
    Task<IReadOnlyCollection<UtxoResponse>> GetUtxosAsync(Guid walletId, CancellationToken cancellationToken);

    /// <summary>
    /// Funds a wallet in regtest mode.
    /// </summary>
    Task<DevFundingResponse> FundWalletAsync(Guid walletId, long amountSatoshis, CancellationToken cancellationToken);
}
