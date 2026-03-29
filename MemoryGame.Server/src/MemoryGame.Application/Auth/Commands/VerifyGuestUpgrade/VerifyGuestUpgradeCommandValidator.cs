using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.VerifyGuestUpgrade;

/// <summary>
/// Validates <see cref="VerifyGuestUpgradeCommand"/>: ensures identifiers are valid,
/// the email format is correct, and the PIN is exactly 6 digits.
/// </summary>
public class VerifyGuestUpgradeCommandValidator : AbstractValidator<VerifyGuestUpgradeCommand>
{
    /// <summary>
    /// Initializes validation rules for guest upgrade verification.
    /// </summary>
    public VerifyGuestUpgradeCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Pin).ValidPin();
    }
}
