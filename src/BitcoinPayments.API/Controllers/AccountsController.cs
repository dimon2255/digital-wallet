using BitcoinPayments.API.Contracts;
using BitcoinPayments.Domain.Entities;
using BitcoinPayments.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinPayments.API.Controllers;

/// <summary>
/// Handles user registration, login, and token refresh.
/// </summary>
[ApiController]
[Route("api/v1/accounts")]
public sealed class AccountsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly JwtTokenService jwtTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountsController"/> class.
    /// </summary>
    public AccountsController(UserManager<ApplicationUser> userManager, JwtTokenService jwtTokenService)
    {
        this.userManager = userManager;
        this.jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(await BuildAuthResponseAsync(user, cancellationToken));
    }

    /// <summary>
    /// Authenticates a user.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new { error = new { code = "INVALID_CREDENTIALS", message = "Invalid email or password." } });
        }

        return Ok(await BuildAuthResponseAsync(user, cancellationToken));
    }

    /// <summary>
    /// Refreshes an access token.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var refreshToken = await jwtTokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (refreshToken is null)
        {
            return Unauthorized(new { error = new { code = "INVALID_REFRESH_TOKEN", message = "Refresh token is invalid or expired." } });
        }

        var user = await userManager.FindByIdAsync(refreshToken.UserId);
        if (user is null)
        {
            return Unauthorized(new { error = new { code = "USER_NOT_FOUND", message = "User not found." } });
        }

        return Ok(await BuildAuthResponseAsync(user, cancellationToken));
    }

    /// <summary>
    /// Authenticates as the demo user (non-production only).
    /// </summary>
    [HttpPost("demo-login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthResponse>> DemoLoginAsync(
        [FromServices] IHostEnvironment env,
        CancellationToken cancellationToken)
    {
        if (env.IsProduction())
        {
            return NotFound();
        }

        var user = await userManager.FindByEmailAsync(DemoSeedService.DemoEmail);
        if (user is null)
        {
            return NotFound(new { error = new { code = "DEMO_NOT_AVAILABLE", message = "Demo user has not been seeded." } });
        }

        return Ok(await BuildAuthResponseAsync(user, cancellationToken));
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshToken = await jwtTokenService.GenerateRefreshTokenAsync(user.Id, cancellationToken);
        var expiresAt = jwtTokenService.GetAccessTokenExpiration();

        return new AuthResponse(
            accessToken,
            refreshToken,
            expiresAt,
            new UserInfo(user.Id, user.Email ?? string.Empty, user.DisplayName));
    }
}
