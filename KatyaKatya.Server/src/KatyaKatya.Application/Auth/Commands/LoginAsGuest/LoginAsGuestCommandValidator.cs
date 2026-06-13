using FluentValidation;
using KatyaKatya.Application.Common.Validators;

namespace KatyaKatya.Application.Auth.Commands.LoginAsGuest;

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
