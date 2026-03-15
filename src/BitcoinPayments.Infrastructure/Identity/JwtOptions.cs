namespace BitcoinPayments.Infrastructure.Identity;

/// <summary>
/// JWT configuration options.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Gets or sets the signing secret.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token issuer.
    /// </summary>
    public string Issuer { get; set; } = "ChainVault";

    /// <summary>
    /// Gets or sets the token audience.
    /// </summary>
    public string Audience { get; set; } = "ChainVault.SPA";

    /// <summary>
    /// Gets or sets access token lifetime in minutes.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
