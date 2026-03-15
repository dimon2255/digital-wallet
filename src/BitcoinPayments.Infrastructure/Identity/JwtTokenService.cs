using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BitcoinPayments.Infrastructure.Identity;

/// <summary>
/// Generates and validates JWT access and refresh tokens.
/// </summary>
public sealed class JwtTokenService
{
    private readonly JwtOptions options;
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    public JwtTokenService(IOptions<JwtOptions> options, AppDbContext dbContext)
    {
        this.options = options.Value;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Generates an access token for the given user.
    /// </summary>
    public string GenerateAccessToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("display_name", user.DisplayName ?? string.Empty),
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gets the access token expiration time.
    /// </summary>
    public DateTimeOffset GetAccessTokenExpiration() =>
        DateTimeOffset.UtcNow.AddMinutes(options.AccessTokenExpirationMinutes);

    /// <summary>
    /// Creates and persists a refresh token for the given user.
    /// </summary>
    public async Task<string> GenerateRefreshTokenAsync(string userId, CancellationToken cancellationToken)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(options.RefreshTokenExpirationDays),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }

    /// <summary>
    /// Validates and rotates a refresh token.
    /// </summary>
    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        var refreshToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return null;
        }

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }
}
