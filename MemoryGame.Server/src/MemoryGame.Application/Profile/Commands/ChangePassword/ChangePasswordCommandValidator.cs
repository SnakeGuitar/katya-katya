using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Profile.Commands.ChangePassword;

/// <summary>
/// Validates <see cref="ChangePasswordCommand"/>: ensures the user identifier is valid,
/// the current password is provided, and the new password meets complexity requirements.
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>
    /// Initializes validation rules for password change.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).ValidId();

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("VALIDATION_CURRENT_PASSWORD_REQUIRED");

        RuleFor(x => x.NewPassword).ValidPassword();
    }
}
