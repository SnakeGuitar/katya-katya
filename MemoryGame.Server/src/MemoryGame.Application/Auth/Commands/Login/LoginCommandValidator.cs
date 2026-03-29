using FluentValidation;

namespace MemoryGame.Application.Auth.Commands.Login;

/// <summary>
/// Validates <see cref="LoginCommand"/>: ensures credentials are provided
/// before attempting authentication.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes validation rules for user login.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("VALIDATION_USERNAME_REQUIRED");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("VALIDATION_PASSWORD_REQUIRED");
    }
}
