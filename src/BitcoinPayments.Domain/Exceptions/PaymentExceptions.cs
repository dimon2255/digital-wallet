namespace BitcoinPayments.Domain.Exceptions;

/// <summary>
/// Raised when a wallet cannot cover the requested amount and fees.
/// </summary>
public sealed class InsufficientFundsException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsufficientFundsException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public InsufficientFundsException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when an escrow cannot be found.
/// </summary>
public sealed class EscrowNotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EscrowNotFoundException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public EscrowNotFoundException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when an operation is not valid for the current entity state.
/// </summary>
public sealed class InvalidOperationStateException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOperationStateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public InvalidOperationStateException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when an authorization can no longer be captured or voided.
/// </summary>
public sealed class AuthorizationExpiredException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationExpiredException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public AuthorizationExpiredException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when a transaction cannot be broadcast to the configured Bitcoin network.
/// </summary>
public sealed class TransactionBroadcastException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBroadcastException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public TransactionBroadcastException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when an idempotency key conflicts with an existing operation.
/// </summary>
public sealed class DuplicateOperationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateOperationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public DuplicateOperationException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when the application attempts to start or operate on mainnet.
/// </summary>
public sealed class MainnetGuardException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainnetGuardException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public MainnetGuardException(string message)
        : base(message)
    {
    }
}
