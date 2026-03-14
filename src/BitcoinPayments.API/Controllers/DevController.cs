using BitcoinPayments.API.Contracts;
using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinPayments.API.Controllers;

/// <summary>
/// Exposes development-only endpoints.
/// </summary>
[ApiController]
[Route("api/dev")]
public sealed class DevController : ControllerBase
{
    private readonly IHostEnvironment hostEnvironment;
    private readonly IWalletApplicationService walletApplicationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DevController"/> class.
    /// </summary>
    public DevController(IHostEnvironment hostEnvironment, IWalletApplicationService walletApplicationService)
    {
        this.hostEnvironment = hostEnvironment;
        this.walletApplicationService = walletApplicationService;
    }

    /// <summary>
    /// Funds a wallet in non-production environments.
    /// </summary>
    [HttpPost("fund-wallet")]
    [ProducesResponseType(typeof(DevFundingResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DevFundingResponse>> FundWalletAsync([FromBody] FundWalletRequest request, CancellationToken cancellationToken)
    {
        if (hostEnvironment.IsProduction())
        {
            return NotFound();
        }

        var response = await walletApplicationService.FundWalletAsync(request.WalletId, request.AmountSatoshis, cancellationToken);
        return Ok(response);
    }
}
