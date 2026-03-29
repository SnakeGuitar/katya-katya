using FluentValidation;
using MemoryGame.Application.Common.Validators;

namespace MemoryGame.Application.Auth.Commands.LoginAsGuest;

/// <summary>
/// Validates <see cref="LoginAsGuestCommand"/>: ensures the guest username
/// meets format and length requirements.
/// </summary>
public class LoginAsGuestCommandValidator : AbstractValidator<LoginAsGuestCommand>
{
    /// <summary>
    /// Initializes validation rules for guest login.
    /// </summary>
    public LoginAsGuestCommandValidator()
    {
        RuleFor(x => x.GuestUsername).ValidUsername();
    }
}
