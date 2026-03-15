using System.Security.Claims;
using BitcoinPayments.API.Contracts;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinPayments.API.Controllers;

/// <summary>
/// Exposes wallet-to-wallet transfer endpoints.
/// </summary>
[ApiController]
[Route("api/v1/transfers")]
[Authorize]
public sealed class TransfersController : ControllerBase
{
    private readonly ISender sender;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransfersController"/> class.
    /// </summary>
    public TransfersController(ISender sender)
    {
        this.sender = sender;
    }

    /// <summary>
    /// Creates a wallet-to-wallet transfer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public Task<PaymentResponse> CreateAsync(
        [FromBody] CreateTransferRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found.");

        return sender.Send(
            new CreateTransferCommand(
                request.SourceWalletId,
                request.DestinationWalletId,
                request.AmountSatoshis,
                request.FeeRateSatPerByte,
                idempotencyKey,
                userId),
            cancellationToken);
    }
}
