namespace BitcoinPayments.Domain.Exceptions;

/// <summary>
/// Thrown when a user attempts to access a resource they do not own.
/// </summary>
public sealed class ForbiddenAccessException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenAccessException"/> class.
    /// </summary>
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
