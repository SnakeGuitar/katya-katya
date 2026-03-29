using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.VerifyRegistration;

/// <summary>
/// Validates <see cref="VerifyRegistrationCommand"/>: ensures the email format
/// is valid and the PIN is exactly 6 digits.
/// </summary>
public class VerifyRegistrationCommandValidator : AbstractValidator<VerifyRegistrationCommand>
{
    /// <summary>
    /// Initializes validation rules for registration verification.
    /// </summary>
    public VerifyRegistrationCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Pin).ValidPin();
    }
}
