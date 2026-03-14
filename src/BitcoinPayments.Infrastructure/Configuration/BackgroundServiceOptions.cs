namespace BitcoinPayments.Infrastructure.Configuration;

/// <summary>
/// Defines background worker polling intervals.
/// </summary>
public sealed class BackgroundServiceOptions
{
    /// <summary>
    /// Gets or sets the confirmation watcher interval in seconds.
    /// </summary>
    public int ConfirmationWatcherIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the auth expiry watcher interval in seconds.
    /// </summary>
    public int AuthExpiryCheckIntervalSeconds { get; set; } = 60;
}
