using BitcoinPayments.Application.Commands;
using FluentValidation;

namespace BitcoinPayments.Application.Validators;

/// <summary>
/// Validates transfer commands.
/// </summary>
public sealed class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTransferCommandValidator"/> class.
    /// </summary>
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.AmountSatoshis).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.SourceWalletId).NotEmpty().WithMessage("Source wallet is required.");
        RuleFor(x => x.DestinationWalletId).NotEmpty().WithMessage("Destination wallet is required.");
        RuleFor(x => x)
            .Must(x => x.SourceWalletId != x.DestinationWalletId)
            .WithMessage("Source and destination wallets must be different.");
    }
}
