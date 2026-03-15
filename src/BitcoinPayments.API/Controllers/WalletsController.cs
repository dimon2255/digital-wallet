using System.Security.Claims;
using BitcoinPayments.API.Contracts;
using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinPayments.API.Controllers;

/// <summary>
/// Exposes wallet management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/wallets")]
[Authorize]
public sealed class WalletsController : ControllerBase
{
    private readonly IWalletApplicationService walletApplicationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WalletsController"/> class.
    /// </summary>
    public WalletsController(IWalletApplicationService walletApplicationService)
    {
        this.walletApplicationService = walletApplicationService;
    }

    /// <summary>
    /// Lists wallets for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<WalletResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<WalletResponse>> ListAsync(CancellationToken cancellationToken) =>
        walletApplicationService.ListWalletsAsync(GetUserId(), cancellationToken);

    /// <summary>
    /// Creates a wallet.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    public Task<WalletResponse> CreateAsync([FromBody] CreateWalletRequest request, CancellationToken cancellationToken) =>
        walletApplicationService.CreateWalletAsync(request.Name, GetUserId(), cancellationToken);

    /// <summary>
    /// Gets a wallet.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    public Task<WalletResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        walletApplicationService.GetWalletAsync(id, GetUserId(), cancellationToken);

    /// <summary>
    /// Generates the next receiving address.
    /// </summary>
    [HttpGet("{id:guid}/address")]
    [ProducesResponseType(typeof(WalletAddressResponse), StatusCodes.Status200OK)]
    public Task<WalletAddressResponse> GetAddressAsync(Guid id, CancellationToken cancellationToken) =>
        walletApplicationService.GetNextReceivingAddressAsync(id, cancellationToken);

    /// <summary>
    /// Lists wallet UTXOs.
    /// </summary>
    [HttpGet("{id:guid}/utxos")]
    [ProducesResponseType(typeof(IReadOnlyCollection<UtxoResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<UtxoResponse>> GetUtxosAsync(Guid id, CancellationToken cancellationToken) =>
        walletApplicationService.GetUtxosAsync(id, cancellationToken);

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("User identity not found.");
}
