using BitcoinPayments.Application.Abstractions;
using BitcoinPayments.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinPayments.API.Controllers;

/// <summary>
/// Exposes transaction lookup endpoints.
/// </summary>
[ApiController]
[Route("api/v1/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private readonly IPaymentQueryService paymentQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionsController"/> class.
    /// </summary>
    public TransactionsController(IPaymentQueryService paymentQueryService)
    {
        this.paymentQueryService = paymentQueryService;
    }

    /// <summary>
    /// Gets a transaction status.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionStatusResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await paymentQueryService.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    /// <summary>
    /// Gets transaction history.
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TransactionStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TransactionStatusResponse>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await paymentQueryService.GetHistoryAsync(id, cancellationToken);
        return response.Count == 0 ? NotFound() : Ok(response);
    }
}
