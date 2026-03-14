using BitcoinPayments.Domain.Exceptions;

namespace BitcoinPayments.API.Middleware;

/// <summary>
/// Converts application exceptions into the platform error contract.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", context.Request.Method, context.Request.Path);

            var (statusCode, errorCode) = exception switch
            {
                InsufficientFundsException => (StatusCodes.Status400BadRequest, "INSUFFICIENT_FUNDS"),
                EscrowNotFoundException => (StatusCodes.Status404NotFound, "ESCROW_NOT_FOUND"),
                InvalidOperationStateException => (StatusCodes.Status409Conflict, "INVALID_OPERATION_STATE"),
                AuthorizationExpiredException => (StatusCodes.Status409Conflict, "AUTHORIZATION_EXPIRED"),
                TransactionBroadcastException => (StatusCodes.Status502BadGateway, "TRANSACTION_BROADCAST_FAILED"),
                DuplicateOperationException => (StatusCodes.Status409Conflict, "DUPLICATE_OPERATION"),
                MainnetGuardException => (StatusCodes.Status500InternalServerError, "MAINNET_GUARD"),
                FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "VALIDATION_FAILED"),
                _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR"),
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                error = new
                {
                    code = errorCode,
                    message = exception.Message,
                },
            };

            await context.Response.WriteAsJsonAsync(payload);
        }
    }
}
