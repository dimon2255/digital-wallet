using System.Text;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Domain.Interfaces;

namespace BitcoinPayments.API.Middleware;

/// <summary>
/// Enforces idempotency for mutating endpoints.
/// </summary>
public sealed class IdempotencyMiddleware
{
    private readonly RequestDelegate next;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdempotencyMiddleware"/> class.
    /// </summary>
    public IdempotencyMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IIdempotencyKeyRepository repository)
    {
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method) &&
            !HttpMethods.IsDelete(context.Request.Method))
        {
            await next(context);
            return;
        }

        // Skip idempotency for auth endpoints
        if (context.Request.Path.StartsWithSegments("/api/v1/accounts"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var values) || string.IsNullOrWhiteSpace(values))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "IDEMPOTENCY_KEY_REQUIRED",
                    message = "Every mutating endpoint requires an Idempotency-Key header.",
                },
            });
            return;
        }

        var key = values.ToString();
        var existing = await repository.GetAsync(key, context.RequestAborted);
        if (existing is not null && existing.ExpiresAt > DateTimeOffset.UtcNow)
        {
            context.Response.StatusCode = existing.ResponseStatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(existing.ResponseBody, Encoding.UTF8, context.RequestAborted);
            return;
        }

        var originalBody = context.Response.Body;
        await using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await next(context);

        memoryStream.Position = 0;
        var responseBody = await new StreamReader(memoryStream, Encoding.UTF8).ReadToEndAsync(context.RequestAborted);
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBody, context.RequestAborted);
        context.Response.Body = originalBody;

        if (context.Response.StatusCode < 500)
        {
            await repository.UpsertAsync(
                new IdempotencyKeyRecord
                {
                    Key = key,
                    ResponseStatusCode = context.Response.StatusCode,
                    ResponseBody = responseBody,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
                },
                context.RequestAborted);
        }
    }
}
