using FluentValidation;
using KatyaKatya.Application.Common.Validators;

namespace KatyaKatya.Application.Auth.Commands.FinalizeRegistration;

/// <summary>
/// Validates <see cref="FinalizeRegistrationCommand"/>: ensures the email format
/// is valid and the PIN is exactly 6 digits.
/// </summary>
public class FinalizeRegistrationCommandValidator : AbstractValidator<FinalizeRegistrationCommand>
{
    /// <summary>
    /// Initializes validation rules for registration finalization.
    /// </summary>
    public FinalizeRegistrationCommandValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Pin).ValidPin();
    }
}
