namespace BitcoinPayments.Domain.Entities;

/// <summary>
/// Represents a stored idempotent response.
/// </summary>
public class IdempotencyKeyRecord
{
    /// <summary>
    /// Gets or sets the idempotency key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response status code.
    /// </summary>
    public int ResponseStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the serialized response body.
    /// </summary>
    public string ResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the expiration timestamp.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow.AddHours(24);
}
