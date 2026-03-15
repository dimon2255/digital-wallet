namespace BitcoinPayments.Infrastructure.Identity;

/// <summary>
/// Persisted refresh token entity.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Gets or sets the refresh token identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token value.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiration timestamp.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the revocation timestamp.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Gets a value indicating whether this token is active.
    /// </summary>
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
