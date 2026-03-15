using Microsoft.AspNetCore.Identity;

namespace BitcoinPayments.Domain.Entities;

/// <summary>
/// Application user extending ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the user display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
