using BitcoinPayments.Application.Commands;
using FluentValidation;

namespace BitcoinPayments.Application.Validators;

/// <summary>
/// Contains validation rules for payment commands.
/// </summary>
public sealed class CreateChargeCommandValidator : AbstractValidator<CreateChargeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateChargeCommandValidator"/> class.
    /// </summary>
    public CreateChargeCommandValidator()
    {
        RuleFor(command => command.BuyerWalletId).NotEmpty();
        RuleFor(command => command.MerchantWalletId).NotEmpty();
        RuleFor(command => command.AmountSatoshis).GreaterThan(0);
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}

/// <summary>
/// Validates authorization requests.
/// </summary>
public sealed class CreateAuthCommandValidator : AbstractValidator<CreateAuthCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAuthCommandValidator"/> class.
    /// </summary>
    public CreateAuthCommandValidator()
    {
        RuleFor(command => command.BuyerWalletId).NotEmpty();
        RuleFor(command => command.MerchantWalletId).NotEmpty();
        RuleFor(command => command.AmountSatoshis).GreaterThan(0);
        RuleFor(command => command.AuthWindowBlocks).GreaterThan(0).When(command => command.AuthWindowBlocks.HasValue);
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}

/// <summary>
/// Validates capture requests.
/// </summary>
public sealed class CaptureAuthCommandValidator : AbstractValidator<CaptureAuthCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CaptureAuthCommandValidator"/> class.
    /// </summary>
    public CaptureAuthCommandValidator()
    {
        RuleFor(command => command.AuthorizationId).NotEmpty();
        RuleFor(command => command.AmountSatoshis).GreaterThan(0);
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}

/// <summary>
/// Validates void requests.
/// </summary>
public sealed class VoidAuthCommandValidator : AbstractValidator<VoidAuthCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VoidAuthCommandValidator"/> class.
    /// </summary>
    public VoidAuthCommandValidator()
    {
        RuleFor(command => command.AuthorizationId).NotEmpty();
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}

/// <summary>
/// Validates refund requests.
/// </summary>
public sealed class CreateRefundCommandValidator : AbstractValidator<CreateRefundCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateRefundCommandValidator"/> class.
    /// </summary>
    public CreateRefundCommandValidator()
    {
        RuleFor(command => command.ParentTransactionId).NotEmpty();
        RuleFor(command => command.AmountSatoshis).GreaterThan(0);
        RuleFor(command => command.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}
