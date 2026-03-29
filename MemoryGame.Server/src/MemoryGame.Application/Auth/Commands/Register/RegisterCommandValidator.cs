using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.Register;

/// <summary>
/// Validates <see cref="RegisterCommand"/>: ensures username, email and password
/// meet all format and complexity requirements before attempting registration.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>
    /// Initializes validation rules for user registration.
    /// </summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).ValidUsername();
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).ValidPassword();
    }
}
