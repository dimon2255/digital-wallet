using BitcoinPayments.API.Contracts;
using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.Commands;
using BitcoinPayments.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinPayments.API.Controllers;

/// <summary>
/// Exposes payment operation endpoints.
/// </summary>
[ApiController]
[Route("api/v1/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender sender;
    private readonly IPaymentQueryService paymentQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    public PaymentsController(ISender sender, IPaymentQueryService paymentQueryService)
    {
        this.sender = sender;
        this.paymentQueryService = paymentQueryService;
    }

    /// <summary>
    /// Executes a direct charge.
    /// </summary>
    [HttpPost("charge")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public Task<PaymentResponse> ChargeAsync(
        [FromBody] CreateChargeRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken) =>
        sender.Send(
            new CreateChargeCommand(
                request.BuyerWalletId,
                request.MerchantWalletId,
                request.AmountSatoshis,
                request.FeeRateSatPerByte,
                request.Metadata,
                idempotencyKey),
            cancellationToken);

    /// <summary>
    /// Creates an authorization hold.
    /// </summary>
    [HttpPost("authorize")]
    [ProducesResponseType(typeof(AuthorizationResponse), StatusCodes.Status200OK)]
    public Task<AuthorizationResponse> AuthorizeAsync(
        [FromBody] CreateAuthorizationRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken) =>
        sender.Send(
            new CreateAuthCommand(
                request.BuyerWalletId,
                request.MerchantWalletId,
                request.AmountSatoshis,
                request.AuthWindowBlocks,
                request.FeeRateSatPerByte,
                request.Metadata,
                idempotencyKey),
            cancellationToken);

    /// <summary>
    /// Captures a previous authorization.
    /// </summary>
    [HttpPost("capture")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public Task<PaymentResponse> CaptureAsync(
        [FromBody] CaptureAuthorizationRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken) =>
        sender.Send(
            new CaptureAuthCommand(
                request.AuthorizationId,
                request.AmountSatoshis,
                request.FeeRateSatPerByte,
                request.Metadata,
                idempotencyKey),
            cancellationToken);

    /// <summary>
    /// Voids a previous authorization.
    /// </summary>
    [HttpPost("void")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public Task<PaymentResponse> VoidAsync(
        [FromBody] VoidAuthorizationRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken) =>
        sender.Send(
            new VoidAuthCommand(
                request.AuthorizationId,
                request.FeeRateSatPerByte,
                request.Metadata,
                idempotencyKey),
            cancellationToken);

    /// <summary>
    /// Creates a refund for a settled charge or capture.
    /// </summary>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public Task<PaymentResponse> RefundAsync(
        [FromBody] CreateRefundRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken) =>
        sender.Send(
            new CreateRefundCommand(
                request.ParentTransactionId,
                request.AmountSatoshis,
                request.FeeRateSatPerByte,
                request.Metadata,
                idempotencyKey),
            cancellationToken);

    /// <summary>
    /// Gets a payment status by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionStatusResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await paymentQueryService.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>
    /// Gets a payment and its related history.
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TransactionStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TransactionStatusResponse>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await paymentQueryService.GetHistoryAsync(id, cancellationToken);
        return response.Count == 0 ? NotFound() : Ok(response);
    }
}
